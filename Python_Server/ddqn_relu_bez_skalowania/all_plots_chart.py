import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

if __name__ == '__main__':
    files = ['history_ddqn_1000', 'history_ddqn_2000', 'history_ddqn_3000',
             'history_ddqn_4000', 'history_ddqn_5000', 'history_ddqn_6000',
             'history_ddqn_7000', 'history_ddqn_8000', 'history_ddqn_9000',
             'history_ddqn_10000']  # 'history_experience_buffer'
    files_array = []
    flat_loss = []
    for f in files:
        obj = pd.read_pickle(f)
        tmp_loss = [d.get('loss', None) for d in obj]
        tmp_flat_loss = [item for sublist in tmp_loss for item in sublist]
        flat_loss.extend(tmp_flat_loss)

    x = np.linspace(1, len(flat_loss), len(flat_loss))
    plt.plot(x, flat_loss)
    plt.title("Loss function for ddqn agent with during 10000 epochs")
    plt.xlabel("Agent's decisions")
    plt.ylabel("Loss")
    plt.savefig('full_plot.png', dpi=300, bbox_inches='tight')
    plt.show()
