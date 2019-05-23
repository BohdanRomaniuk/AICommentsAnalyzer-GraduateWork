import sys
import pandas as pd
from keras.preprocessing.text import Tokenizer
from keras.preprocessing.sequence import pad_sequences
from keras.models import load_model

model = load_model('./ai/my_model.h5')
df_test = pd.read_csv(sys.argv[1], delimiter=",")
df_test = df_test.drop(['id'], axis=1)
list_sentences_test = df_test["review"]

max_features = 6000
tokenizer = Tokenizer(num_words=max_features)
list_tokenized_test = tokenizer.texts_to_sequences(list_sentences_test)

maxlen = 130
X_te = pad_sequences(list_tokenized_test, maxlen=maxlen)
prediction = model.predict(X_te)

print(prediction)