#region license
// Transformalize
// A Configurable ETL Solution Specializing in Incremental Denormalization.
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

using System.Collections.Generic;
using Autofac;
using Pipeline.Configuration;

namespace JunkDrawer.Autofac.Modules {
    public abstract class ConnectionModule : Module {
        readonly IEnumerable<Connection> _connections;

        protected ConnectionModule() {}

        protected ConnectionModule(IEnumerable<Connection> connections) {
            _connections = connections;
        }

        protected abstract void RegisterConnection(ContainerBuilder builder, Connection connection);

        protected override void Load(ContainerBuilder builder) {
            if (_connections == null)
                return;
            foreach (var c in _connections) {
                RegisterConnection(builder, c);
            }
        }
    }
}