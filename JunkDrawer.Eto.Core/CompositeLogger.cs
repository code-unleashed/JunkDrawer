#region license
// JunkDrawer.Eto.Core
// Copyright 2013 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System;
using Transformalize.Context;
using Transformalize.Contracts;

namespace JunkDrawer.Eto.Core {
    public class CompositeLogger : IPipelineLogger {
        private readonly IPipelineLogger[] _loggers;

        public CompositeLogger(params IPipelineLogger[] loggers) {
            _loggers = loggers;
        }

        public void Debug(PipelineContext context, Func<string> lambda) {
            foreach (var logger in _loggers) {
                logger.Debug(context, lambda);
            }
        }

        public void Info(PipelineContext context, string message, params object[] args) {
            foreach (var logger in _loggers) {
                logger.Info(context, message, args);
            }
        }

        public void Warn(PipelineContext context, string message, params object[] args) {
            foreach (var logger in _loggers) {
                logger.Warn(context, message, args);
            }
        }

        public void Error(PipelineContext context, string message, params object[] args) {
            foreach (var logger in _loggers) {
                logger.Error(context, message, args);
            }
        }

        public void Error(PipelineContext context, Exception exception, string message, params object[] args) {
            foreach (var logger in _loggers) {
                logger.Error(context, exception, message, args);
            }
        }

        public void Clear() {
            foreach (var logger in _loggers) {
                logger.Clear();
            }
        }

        public LogLevel LogLevel { get; }
    }
}