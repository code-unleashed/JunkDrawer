﻿#region license
// JunkDrawer.Autofac
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

using Pipeline.Contracts;

namespace JunkDrawer.Autofac {
    public class AutofacJunkBootstrapperFactory : IJunkBootstrapperFactory {
        private readonly IPipelineLogger _logger;

        public AutofacJunkBootstrapperFactory(IPipelineLogger logger = null) {
            _logger = logger;
        }

        public IJunkBootstrapper Produce(JunkRequest request) {
            return new AutofacJunkBootstrapper(request, _logger);
        }

        public IJunkBootstrapper Produce() {
            return new AutofacJunkBootstrapper(_logger);
        }
    }
}