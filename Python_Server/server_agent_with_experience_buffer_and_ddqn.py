import time
import zmq
import random
import math
import os.path
import numpy as np
import tensorflow as tf
from keras.models import Sequential
from keras.layers.core import Dense, Activation
from keras.utils import np_utils
from keras.optimizers import Adam
from keras.models import load_model
import pickle


# defines about game termination
PLAY = 0
WIN = 1
LOSE = -1

#defines about action
RESTART = 3

#defines about model
GAMES = 1000 #number of games in unity
HIDDEN_LAYERS = 800
LEARNING_RATE = 0.00002 #step in function plane (function, which is approximated by nn)
EPS_MAX = 1.0 #max probablity of random action
EPS_MIN = 0.1 #min probability of random action
EPS_REDUCE = 0.999 #to reduce probability of random action each game
GAMMA = 0.99 #const, that means how the future action are important

class ReplayBuffer():
    def __init__(self, max_size, input_dims):
        self.mem_size = max_size
        self.mem_cntr = 0

        self.state_memory = np.zeros((self.mem_size, *input_dims), dtype=np.float32)
        self.new_state_memory = np.zeros((self.mem_size, *input_dims), dtype=np.float32)
        self.action_memory = np.zeros(self.mem_size, dtype=np.int32)
        self.reward_memory = np.zeros(self.mem_size, dtype=np.float32)
        self.terminal_memory = np.zeros(self.mem_size, dtype=np.int32)

    #store experience of agent
    def store_experience(self, state, action, reward, next_state, done):
        index = self.mem_cntr % self.mem_size
        self.state_memory[index] = state
        self.new_state_memory[index] = next_state
        self.reward_memory[index] = reward
        self.action_memory[index] = action
        self.terminal_memory[index] = 1 - int(np.abs(done))
        self.mem_cntr += 1

    #give experience in random order to agent for training
    def get_random_experience(self, batch_size):
        max_mem = min(self.mem_cntr, self.mem_size)
        batch = np.random.choice(max_mem, batch_size, replace=False)

        states = self.state_memory[batch]
        next_states = self.new_state_memory[batch]
        rewards = self.reward_memory[batch]
        actions = self.action_memory[batch]
        terminal = self.terminal_memory[batch]

        return states, actions, rewards, next_states, terminal

#building model
def build_dqn(input_size, output_size):
    model = Sequential()
    model.add(Dense(units=HIDDEN_LAYERS, activation='relu', input_shape=input_size))
    model.add(Dense(units=HIDDEN_LAYERS, activation='relu'))
    model.add(Dense(units=output_size, activation='linear'))
    model.compile(optimizer=Adam(lr=LEARNING_RATE), loss='mse')
    return model

class QAgent:
    def __init__(self, actions_size, batch_size, input_dims, mem_size=1000000,
                 model_file='model_ddqn.h5', history_file='history_ddqn', replace_target=100):
        self.history = []
        self.actions_size = actions_size
        self.action_space = [i for i in range(actions_size)]
        self.epsilon = EPS_MAX
        self.batch_size = batch_size
        self.model_file = model_file
        self.history_file = history_file
        self.replace_target = replace_target
        self.memory = ReplayBuffer(mem_size, input_dims)
        self.q_eval = build_dqn(input_dims, actions_size)
        self.q_target = build_dqn(input_dims, actions_size)

    def store_experience(self, state, action, reward, next_state, done):
        self.memory.store_experience(state,action,reward, next_state, done)

    def get_action(self, state):
        if np.random.random() < self.epsilon:
             action = np.random.choice(self.action_space)
        else:
            state = np.array([state])
            actions = self.q_eval.predict(state)
            action = np.argmax(actions)

        return action

    def train(self):
        if self.memory.mem_cntr < self.batch_size:
            return

        states, actions, rewards, next_states, dones = self.memory.get_random_experience(self.batch_size)

        q_next = self.q_target.predict(next_states)

        q_pred = self.q_eval.predict(states)

        q_target = np.copy(q_pred)
        batch_index = np.arange(self.batch_size, dtype=np.int32)

        q_target[batch_index, actions] = rewards + GAMMA * np.max(q_next, axis=1) * dones

        his = self.q_eval.fit(states, q_target, verbose=0)

        self.history.append(his.history)

        if self.epsilon > EPS_MIN:
            self.epsilon = self.epsilon*EPS_REDUCE
        else:
            self.epsilon = EPS_MIN

        if self.memory.mem_cntr % self.replace_target == 0:
            self.update_network_parameters()

    def update_network_parameters(self):
        self.q_target.model.set_weights(self.q_eval.model.get_weights())

    def save_model(self):
        self.q_eval.save_weights(self.model_file)

    def load_model(self):
        if os.path.isfile(self.model_file):
            self.q_eval.load_weights(self.model_file)
            self.q_target.load_weights(self.model_file)
            return True
        return False

    def save_loss_to_file(self):
        with open(self.history_file, 'wb') as fp:
            pickle.dump(self.history, fp)

    #after training - agent know environment and don't have to explore it
    def presentation_mode(self):
        self.epsilon = EPS_MIN


def parse_message(message):
    # decode and split message from unity with spaces
    splt_help = message.decode().split(' ')

    # replace comma with dot in message
    splt = [sub.replace(',', '.') for sub in splt_help]

    # get information about game from message
    done = int(splt[0])
    reward = int(splt[1])
    state = [float(item) for item in splt[2:]]

    #scaling reward
    return done, math.tanh(reward), np.array(state)

if __name__ == '__main__':
    context = zmq.Context()
    socket = context.socket(zmq.REP)
    socket.bind("tcp://*:5555")
    agent = QAgent(actions_size=3, input_dims=[69], batch_size=64)
    if agent.load_model():
        agent.presentation_mode()
    message = socket.recv()
    restart = False
    done, reward, state = parse_message(message)
    iterator = 0
    while(iterator < GAMES):

        if done != PLAY and restart is False:
            action = RESTART
            agent.save_model()
            iterator += 1
            restart = True

        elif done == PLAY:
            action = agent.get_action(state)
            restart = False

        socket.send_string(str(action))
        message = socket.recv()
        done, reward, next_state = parse_message(message)
        if action != RESTART:
            agent.store_experience(state, action, reward, next_state, done)
            agent.train()
        state = next_state
    agent.save_loss_to_file()






