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
using Cfg.Net.Contracts;
using Cfg.Net.Environment;
using Cfg.Net.Reader;
using Transformalize;
using Transformalize.Configuration;
using Transformalize.Contracts;

// ReSharper disable PossibleMultipleEnumeration

namespace JunkDrawer.Autofac.Modules {

    public class RootModule : Module {

        protected override void Load(ContainerBuilder builder) {

            // This reader is used to load the initial configuration and nested resources for tfl actions, etc.
            builder.RegisterType<FileReader>().Named<IReader>("file");
            builder.RegisterType<WebReader>().Named<IReader>("web");
            builder.Register<IReader>(ctx => new DefaultReader(
                ctx.ResolveNamed<IReader>("file"),
                new ReTryingReader(ctx.ResolveNamed<IReader>("web"), attempts: 3))
            );

            builder.Register((ctx, p) => {
                var process = new Process(new List<IDependency> {
                    ctx.Resolve<IReader>(),
                    new EnvironmentModifier()
                }.ToArray());

                switch (p.Count()) {
                    case 2:
                        process.Load(
                            p.Named<string>("cfg"),
                            p.Named<Dictionary<string, string>>("parameters")
                        );
                        break;
                    default:
                        process.Load(p.Named<string>("cfg"));
                        break;
                }

                // this might be put into it's own type and injected (or not)
                if (process.Entities.Count == 1) {
                    var entity = process.Entities.First();
                    if (!entity.HasInput() && ctx.IsRegistered<ISchemaReader>()) {
                        var schemaReader = ctx.Resolve<ISchemaReader>(new TypedParameter(typeof(Process), process));
                        var newEntity = schemaReader.Read(entity).Entities.First();
                        foreach (var sf in newEntity.Fields.Where(f => f.Name == Constants.TflKey || f.Name == Constants.TflDeleted || f.Name == Constants.TflBatchId || f.Name == Constants.TflHashCode)) {
                            sf.Alias = newEntity.Name + sf.Name;
                        }
                        process.Entities.Clear();
                        process.Entities.Add(newEntity);
                        process = new Process(process.Serialize(), ctx.Resolve<ISerializer>());
                    }
                }

                return process;
            }).As<Process>().InstancePerDependency();  // because it has state, if you run it again, it's not so good

        }
    }
}
