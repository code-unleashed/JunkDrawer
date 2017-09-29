﻿#region license
// JunkDrawer
// An easier way to import excel or delimited files into a database.
// Copyright 2013-2017 Dale Newman
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
using System.Linq;
using Autofac;
using Transformalize;
using Transformalize.Actions;
using Transformalize.Configuration;
using Transformalize.Context;
using Transformalize.Contracts;
using Transformalize.Impl;
using Transformalize.Providers.Ado;

namespace JunkDrawer.Autofac.Modules {
    public class ProcessControlModule : Module {
        private readonly Process _process;

        public ProcessControlModule() { }

        public ProcessControlModule(Process process) {
            _process = process;
        }

        protected override void Load(ContainerBuilder builder) {

            if (_process == null)
                return;

            if (!_process.Enabled)
                return;

            builder.Register<IProcessController>(ctx => {

                var pipelines = new List<IPipeline>();

                // entity-level pipelines
                foreach (var entity in _process.Entities) {
                    var pipeline = ctx.ResolveNamed<IPipeline>(entity.Key);

                    pipelines.Add(pipeline);
                    if (entity.Delete && _process.Mode != "init") {
                        pipeline.Register(ctx.ResolveNamed<IEntityDeleteHandler>(entity.Key));
                    }
                }

                // process-level pipeline for process level calculated fields
                if (ctx.IsRegistered<IPipeline>()) {
                    pipelines.Add(ctx.Resolve<IPipeline>());
                }

                var outputConnection = _process.Output();
                var context = ctx.Resolve<IContext>();

                var controller = new ProcessController(pipelines, context);

                // output initialization
                if (_process.Mode == "init") {
                    var output = ctx.ResolveNamed<OutputContext>(outputConnection.Key);
                    switch (outputConnection.Provider) {
                        case "mysql":
                        case "postgresql":
                        case "sqlite":
                        case "sqlce":
                        case "sqlserver":
                        case "elastic":
                        case "lucene":
                            controller.PreActions.Add(ctx.Resolve<IInitializer>());
                            break;
                        default:
                            output.Debug(()=>$"The {outputConnection.Provider} provider does not support initialization.");
                            break;
                    }

                }

                // flatten
                var o = ctx.ResolveNamed<OutputContext>(outputConnection.Key);
                if (_process.Flatten && Constants.AdoProviderSet().Contains(o.Connection.Provider)) {
                    controller.PostActions.Add(new AdoFlattenAction(o, ctx.ResolveNamed<IConnectionFactory>(outputConnection.Key)));
                }

                // actions
                foreach (var action in _process.Actions.Where(a => a.GetModes().Any(m => m == _process.Mode || m == "*"))) {
                    if (action.Before) {
                        controller.PreActions.Add(ctx.ResolveNamed<IAction>(action.Key));
                    }
                    if (action.After) {
                        controller.PostActions.Add(ctx.ResolveNamed<IAction>(action.Key));
                    }
                }

                foreach (var map in _process.Maps.Where(m => !string.IsNullOrEmpty(m.Query))) {
                    controller.PreActions.Add(new MapReaderAction(context, map, ctx.ResolveNamed<IMapReader>(map.Name)));
                }

                return controller;
            }).As<IProcessController>();

        }

    }
}