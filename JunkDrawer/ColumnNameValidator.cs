using System;
using System.Collections.Generic;
using System.Linq;

namespace JunkDrawer {
    public class ColumnNameValidator {
        private readonly string[] _names;
        private readonly int _count;
        private readonly int _distinctCount;

        public ColumnNameValidator(IEnumerable<string> names) {
            _names = names.ToArray();
            _count = _names.Count();
            _distinctCount = _names.Distinct().Count();
        }

        public bool Valid() {
            return AreDistinct() &&
                !ContainEmptyOrWhiteSpace() &&
                !ContainNumber() &&
                !ContainDatetime() &&
                !ContainGuid();
        }

        private bool AreDistinct() {
            return _count == _distinctCount;
        }

        private bool ContainEmptyOrWhiteSpace() {
            return _names.Any(n => string.IsNullOrEmpty(n) || string.IsNullOrWhiteSpace(n));
        }

        private bool ContainNumber() {
            float number;
            return _names.Any(n => float.TryParse(n, out number));
        }

        private bool ContainDatetime() {
            DateTime date;
            return _names.Any(n => DateTime.TryParse(n, out date));
        }

        private bool ContainGuid() {
            Guid guid;
            return _names.Any(n => Guid.TryParse(n, out guid));
        }

    }
}