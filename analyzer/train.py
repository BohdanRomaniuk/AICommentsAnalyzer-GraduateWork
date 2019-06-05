import sys
import os
import pandas as pd
import warnings
import tensorflow as tf
import pickle
from imp import reload
from keras.preprocessing.text import Tokenizer
from keras.preprocessing.sequence import pad_sequences
from keras.layers import Dense, LSTM, Embedding, Dropout
from keras.layers import Bidirectional, GlobalMaxPool1D
from keras.models import Sequential

tf.logging.set_verbosity(tf.logging.FATAL)
warnings.filterwarnings('ignore')

df = pd.read_csv(sys.argv[1], delimiter="\t")
df = df.drop(['id'], axis=1)

max_features = 19000
tokenizer = Tokenizer(num_words=max_features)
tokenizer.fit_on_texts(df['review'])
list_tokenized_train = tokenizer.texts_to_sequences(df['review'])

maxlen = 130
X_t = pad_sequences(list_tokenized_train, maxlen=maxlen)
y = df['sentiment']

embed_size = 128
model = Sequential()
model.add(Embedding(max_features, embed_size))
model.add(Bidirectional(LSTM(32, return_sequences = True)))
model.add(GlobalMaxPool1D())
model.add(Dense(20, activation="relu"))
model.add(Dropout(0.05))
model.add(Dense(1, activation="sigmoid"))
model.compile(loss='binary_crossentropy', optimizer='adam', metrics=['accuracy'])

batch_size = 100
epochs = 2
model.fit(X_t,y, batch_size=batch_size, epochs=epochs, validation_split=0.1)

#Saving training network and tokenizer
model.save('./ai/trained_network.h5')
with open('./ai/tokenizer.pickle', 'wb') as handle:
    pickle.dump(tokenizer, handle, protocol=pickle.HIGHEST_PROTOCOL)
print("Finished!")
os.system("pause")