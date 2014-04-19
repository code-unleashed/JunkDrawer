﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JunkDrawer {

    public class LineLoader {
        private readonly InspectionRequest _request;
        private readonly FileLineLoader _loader;
        private readonly bool _isCsv;

        public LineLoader(FileSystemInfo fileInfo, InspectionRequest request) {
            _request = request;
            _loader = new FileLineLoader(fileInfo.FullName);
            _isCsv = fileInfo.Extension.Equals(".csv", StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<Line> Load() {
            return _isCsv ?
                _loader.Load().Select(content => new Line(content, '\"', _request)) :
                _loader.Load().Select(content => new Line(content, _request));
        }
    }
}