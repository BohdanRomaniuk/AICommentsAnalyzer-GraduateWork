import sys
import pickle
import numpy
from keras.preprocessing.sequence import pad_sequences
from keras.models import load_model

model = load_model('./ai/trained_network.h5')
list_sentences_test = numpy.array([sys.argv[1]])

with open('./ai/tokenizer.pickle', 'rb') as handle:
    tokenizer = pickle.load(handle)
list_tokenized_test = tokenizer.texts_to_sequences(list_sentences_test)

maxlen = 130
x_test = pad_sequences(list_tokenized_test, maxlen=maxlen)
print(model.predict(x_test))