import sys
import pickle
import numpy
import pandas as pd
import tensorflow as tf
from keras.preprocessing.sequence import pad_sequences
from keras.models import load_model
import warnings

tf.logging.set_verbosity(tf.logging.FATAL)
warnings.filterwarnings('ignore')

model = load_model('./ai/trained_network.h5')
df_test = pd.read_csv(sys.argv[1], delimiter="\t")
list_sentences_test = df_test["review"]

with open('./ai/tokenizer.pickle', 'rb') as handle:
    tokenizer = pickle.load(handle)
list_tokenized_test = tokenizer.texts_to_sequences(list_sentences_test)

maxlen = 130
x_test = pad_sequences(list_tokenized_test, maxlen=maxlen)
prediction = model.predict(x_test)

y_pred = (prediction > 0.5)
df_test["sentiment"] = numpy.where(prediction > 0.5, 1, 0)
df_test.drop(columns=['review']).to_csv("./test/result.tsv", header=1, sep="\t", index=0, quoting=3)