using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Transformalize.Configuration.Builders;
using Transformalize.Libs.NLog;
using Transformalize.Main;

namespace JunkDrawer {

    public class FieldInspector {

        private readonly Logger _log = LogManager.GetLogger(string.Empty);

        public List<Field> Inspect(FileInformation fileInformation, InspectionRequest request) {

            var builder = new ProcessBuilder(Utility.TypeCheckPrefix + fileInformation.Identifier().TrimStart(Utility.ImportPrefix.ToCharArray()))
                .Connection("input")
                    .Provider("file")
                    .File(fileInformation.FileInfo.FullName)
                    .Delimiter(fileInformation.Delimiter == default(char) ? string.Empty : fileInformation.Delimiter.ToString(CultureInfo.InvariantCulture))
                    .Start(fileInformation.FirstRowIsHeader ? 2 : 1)
                .Connection("output")
                    .Provider("internal")
                .Entity("Data").DetectChanges(false)
                    .Sample(request.Sample);

            foreach (var field in fileInformation.Fields) {
                builder
                    .Field(field.Name)
                        .Length(field.Length)
                        .Type(field.Type)
                        .QuotedWith(field.QuoteString());
            }

            foreach (var dataType in request.DataTypes) {
                foreach (var field in fileInformation.Fields) {
                    var result = IsDataTypeField(field.Name, dataType);
                    builder.CalculatedField(result).Bool()
                        .Transform("typeconversion")
                            .Type(dataType)
                            .ResultField(result)
                            .MessageField(string.Empty)
                            .Parameter(field.Name);
                }
            }

            foreach (var field in fileInformation.Fields) {
                var result = field.Name + "Length";
                builder.CalculatedField(result).Int32()
                    .Transform("length")
                    .Parameter(field.Name);
            }

            _log.Debug(builder.Process().Serialize().Replace(Environment.NewLine, string.Empty));

            var runner = ProcessFactory.Create(builder.Process(), new Options() { Top = request.Top })[0];
            var results = runner.Run()["Data"].ToList();

            foreach (var field in fileInformation.Fields) {
                var foundMatch = false;
                foreach (var dataType in request.DataTypes) {
                    var result = IsDataTypeField(field.Name, dataType);
                    if (!foundMatch && results.All(row => row[result].Equals(true))) {
                        field.Type = dataType;
                        field.Length = string.Empty;
                        foundMatch = true;
                    }
                }
                if (!foundMatch) {
                    var length = results.Max(row => (int)row[field.Name + "Length"]) + 1;
                    field.Length = length.ToString(CultureInfo.InvariantCulture);
                }
            }

            return fileInformation.Fields;
        }

        private static string IsDataTypeField(string name, string dataType) {
            return name + "Is" + dataType;
        }

    }
}