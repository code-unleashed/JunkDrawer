﻿using System.Collections.Generic;

namespace JunkDrawer {
    public class Line {

        private readonly char _quote = default(char);
        private readonly string _content = string.Empty;
        private readonly Dictionary<char, string[]> _values = new Dictionary<char, string[]>();

        public string Content {
            get { return _content; }
        }

        public Dictionary<char, string[]> Values {
            get { return _values; }
        }

        public char Quote { get { return _quote; } }

        public Line(string content, InspectionRequest request) {
            _content = content;
            foreach (var delimiter in request.Delimiters.Keys) {
                _values[delimiter] = content.Split(delimiter);
            }
        }

        public Line(string content, char quote, InspectionRequest request) {
            _content = content;
            _quote = quote;
            foreach (var delimiter in request.Delimiters.Keys) {
                _values[delimiter] = content.DelimiterSplit(delimiter, quote);
            }
        }

    }
}