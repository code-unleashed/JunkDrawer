#region license
// JunkDrawer
// Copyright 2013 Dale Newman
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
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
using Pipeline;
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Desktop.Transforms;
using Pipeline.Transforms;
using Pipeline.Transforms.System;
using Pipeline.Validators;

namespace JunkDrawer.Autofac {
    public static class TransformFactory {

        public static IEnumerable<ITransform> GetTransforms(IComponentContext ctx, Process process, Entity entity, IEnumerable<Field> fields) {
            var transforms = new List<ITransform>();
            foreach (var f in fields.Where(f => f.Transforms.Any())) {
                var field = f;
                if (field.RequiresCompositeValidator()) {
                    transforms.Add(new CompositeValidator(
                        new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, field),
                        field.Transforms.Select(t => ShouldRunTransform(ctx, new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, field, t)))
                        ));
                } else {
                    transforms.AddRange(field.Transforms.Select(t => ShouldRunTransform(ctx, new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, field, t))));
                }
            }
            return transforms;
        }

        public static ITransform ShouldRunTransform(IComponentContext ctx, PipelineContext context) {
            return context.Transform.ShouldRun == null ? SwitchTransform(ctx, context) : new ShouldRunTransform(context, SwitchTransform(ctx, context));
        }

        static ITransform SwitchTransform(IComponentContext ctx, PipelineContext context) {

            switch (context.Transform.Method) {
                case "convert": return new ConvertTransform(context);
                case "now": return new NowTransform(context);
                case "toyesno": return new ToYesNoTransform(context);
                case "regexreplace": return new CompiledRegexReplaceTransform(context);
                // (portable) case "regexreplace": return new RegexReplaceTransform(context);
                case "replace": return new ReplaceTransform(context);
                case "formatphone": return new FormatPhoneTransform(context);
                case "utcnow":return new UtcNowTransform(context);
                case "timeago": return new RelativeTimeTransform(context, true);
                case "timeahead": return new RelativeTimeTransform(context, false);
                case "format": return new FormatTransform(context);
                case "substring": return new SubStringTransform(context);
                case "left": return new LeftTransform(context);
                case "right": return new RightTransform(context);
                case "copy": return new CopyTransform(context);
                case "concat": return new ConcatTransform(context);
                case "fromxml": return new FromXmlTransform(context);
                case "fromsplit": return new FromSplitTransform(context);
                case "htmldecode": return new DecodeTransform(context);
                case "xmldecode": return new DecodeTransform(context);
                case "hashcode": return new HashcodeTransform(context);
                case "padleft": return new PadLeftTransform(context);
                case "padright": return new PadRightTransform(context);
                case "splitlength": return new SplitLengthTransform(context);
                case "timezone": return new TimeZoneTransform(context);
                case "trim": return new TrimTransform(context);
                case "trimstart": return new TrimStartTransform(context);
                case "trimend": return new TrimEndTransform(context);
                case "insert": return new InsertTransform(context);
                case "remove": return new RemoveTransform(context);
                case "cs":
                case "csharp": return new CsharpTransform(context);
                case "tostring": return new ToStringTransform(context);
                case "toupper": return new ToUpperTransform(context);
                case "tolower": return new ToLowerTransform(context);
                case "join": return new JoinTransform(context);
                case "map": return new MapTransform(context, ctx.ResolveNamed<IMapReader>(context.Process.Maps.First(m => m.Name == context.Transform.Map).Key));
                case "decompress": return new DecompressTransform(context);
                case "next": return new NextTransform(context);
                case "last": return new LastTransform(context);
                case "datepart": return new DatePartTransform(context);

                case "contains": return new ContainsValidater(context);
                case "is": return new IsValidator(context);

                default:
                    context.Warn("The {0} method is undefined.", context.Transform.Method);
                    return new NullTransformer(context);
            }
        }
    }
}