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
GAMES = 10000 #number of games in unity
HIDDEN_LAYERS = 800
LEARNING_RATE = 0.00002 #step in function plane (function, which is approximated by nn)
EPS_MAX = 1.0 #max probablity of random action
EPS_MIN = 0.1 #min probability of random action
EPS_REDUCE = 0.999 #to reduce probability of random action each game
GAMMA = 0.99 #const, that means how the future action are important


#building model
def build_dqn(input_size, output_size):
    model = Sequential()
    model.add(Dense(units=HIDDEN_LAYERS, activation='relu', input_shape=(input_size,)))
    model.add(Dense(units=HIDDEN_LAYERS, activation='relu'))
    model.add(Dense(units=output_size, activation='linear'))
    model.compile(optimizer=Adam(lr=LEARNING_RATE), loss='mse')
    return model

class QAgent:
    def __init__(self, actions_size,state_size, model_file='model_simple_agent.h5', history_file='history_simple_agent'):
        self.actions_size = actions_size

        #epsilon - probablity of random action; to explore env
        self.epsilon = EPS_MAX
        self.model_file = model_file
        self.state_size = state_size
        self.model = build_dqn(state_size, actions_size)

        #to store training history
        self.history = []
        self.history_file = history_file

    def get_action(self, state):
        guess = np.random.random()
        if guess < self.epsilon:
            action = np.random.randint(0, self.actions_size)
        else:
            action = np.argmax(self.model.predict(state.reshape(1,self.state_size)))
        return action

    def train(self, q_action, reward, done, state, next_state):

        #if it's end of single game
        if done != PLAY or q_action == RESTART:
            self.epsilon = max(self.epsilon * EPS_REDUCE, EPS_MIN)
        else:
            #Bellman equation Q(s,a) = REWARD + GAMMA*MAX(Q(s+1,a))
            q_next_action = self.model.predict(next_state.reshape(1,self.state_size))
            q_val = self.model.predict(state.reshape(1,self.state_size))
            target = q_val.copy()

            #we change only one action; this calculated from Bellman equation; copy rest
            target[0][q_action] = reward + GAMMA * np.argmax(q_next_action)

            #training and saving it
            his = self.model.fit(state.reshape(1,self.state_size), target.reshape(1, self.actions_size), verbose=0)
            self.history.append(his.history)

    def save_model(self):
        self.model.save_weights(self.model_file)

    def load_model(self):
        if os.path.isfile(self.model_file):
            self.model.load_weights(self.model_file)
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
    agent = QAgent(actions_size=3, state_size=69)
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
        agent.train(action, reward, done, state, next_state)
        state = next_state
    agent.save_loss_to_file()
