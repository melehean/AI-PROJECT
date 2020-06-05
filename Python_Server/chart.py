import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
if __name__ == '__main__':
    object = pd.read_pickle(r'history_experience_buffer')
    loss = [d.get('loss', None) for d in object]
    flat_loss = [item for sublist in loss for item in sublist]
    x = np.linspace(1, len(flat_loss), len(flat_loss))
    plt.plot(x,flat_loss)
    plt.savefig('plot.png', dpi=300, bbox_inches='tight')
    plt.show()


