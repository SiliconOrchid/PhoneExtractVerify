using System.Collections.Generic;
using System.Linq;

using PhoneExtractVerify.Api.Services.Interface;

namespace PhoneExtractVerify.Api.Services
{
    public class WordProcessingService : IWordProcessingService
    {
        private static string _countryPrefix = "+44";
        private static int _minWordLength = 10;
        private List<string> _listProcessedWords;


        public List<string> ListProcessedWords
        {
            get { return _listProcessedWords; }
        }


        #region Initiating Method

        /// <summary>
        /// Used to intialise chaining class, by populating with unfiltered collection of "OCR extracted words" from source.
        /// </summary>
        /// <param name="listWords"></param>
        /// <returns></returns>
        public WordProcessingService AddWords(List<string> listWords)
        {
            _listProcessedWords = listWords;
            return this;
        }

        #endregion


        #region Chaining Method(s)

        /// <summary>
        /// Combine sequential numbers, presented in original collection of words, which may in fact fragments of a longer number.
        /// Return as a larger collection of possible words.
        /// </summary>
        /// <returns></returns>
        public WordProcessingService GetCandidatePhoneNumbers()
        {
            bool IsTrailingNumber = false;

            List<string> listCombinedNumberWords = new List<string>();

            foreach (var word in _listProcessedWords)
            {
                listCombinedNumberWords.Add(word);

                if (word.Any(char.IsDigit) || word.Contains("+") || word.Contains("(") || word.Contains(")"))
                {

                    if (IsTrailingNumber)
                        listCombinedNumberWords.Add($"{listCombinedNumberWords[listCombinedNumberWords.Count - 2]}{word}");

                    IsTrailingNumber = true;
                }
                else
                {
                    IsTrailingNumber = false;
                }

            }

            _listProcessedWords = listCombinedNumberWords;
            return this;
        }


        /// <summary>
        /// Filter collection to include only words that contain at least one numeric character.
        /// Intended to discard words that cannot possibly be part of a phone number.
        /// </summary>
        /// <returns></returns>
        public WordProcessingService ExtractWordsWithNumbers()
        {
            _listProcessedWords = _listProcessedWords.Where(word => word.Any(char.IsDigit)).ToList();
            return this;
        }


        /// <summary>
        /// Filter collection to include only words that meet a minimum character length
        /// </summary>
        /// <returns></returns>
        public WordProcessingService GetMinLengthNumbers()
        {
             _listProcessedWords = _listProcessedWords.Where(w => w.Length >= _minWordLength).ToList();
            return this;
        }



        /// <summary>
        /// Attempts to reformat words in collection, from a UK national format and into E.164
        /// </summary>
        /// <returns></returns>
        public WordProcessingService ReformatAsUKInternational()
        {
            List<string> listReformattedNumberWords = new List<string>();

            foreach (var word in _listProcessedWords)
            {
                // strip out non-numeric characters into a copy
                string numbersOnlyWord = GetOnlyNumbers(word);

                //the original word contains a leading "+", suggesting an international-prefix has been supplied - restore just this "+" symbol back to the numerically stripped version
                if (word.Substring(0,1) == "+")
                {
                    listReformattedNumberWords.Add($"+{numbersOnlyWord}");
                    continue;
                }


                // the first character is a zero, suggesting this may be a UK national number, so replace just that leading zero with the internation prefix
                if (numbersOnlyWord.Substring(0, 1) == "0")
                { 
                    listReformattedNumberWords.Add($"{_countryPrefix}{numbersOnlyWord.Substring(1, numbersOnlyWord.Length-1)}");
                    continue;
                }

                // looks like the number starts with something other than zero - we can't really tell what to do with this, so just add the country-code and pass it onwards.
                listReformattedNumberWords.Add($"{_countryPrefix}{numbersOnlyWord}");
            }

            _listProcessedWords = listReformattedNumberWords;

            return this;
        }

        #endregion


        #region Private Method(s)
        /// <summary>
        /// Strip out non-numeric characters from string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string GetOnlyNumbers(string input)
        {
            return new string(input.Where(c => char.IsDigit(c)).ToArray());
        }
        #endregion
    }
}
