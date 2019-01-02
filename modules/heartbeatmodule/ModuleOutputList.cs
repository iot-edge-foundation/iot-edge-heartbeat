namespace iot.edge.heartbeat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ModuleOutputList : List<ModuleOutput>
    {
        public void WriteOutputInfo()
        {
            if (this.Count == 0)
            {
                Console.WriteLine("This module uses outputs ");
            }

            var outputText = "This module uses outputs: ";

            foreach (var item in this)
            {
                outputText += $"'{item.Name}'; ";
            }

            // print text except for the last two characters
            Console.WriteLine(outputText.Substring(0, outputText.Length - 2));
        }

        public ModuleOutput GetModuleOutput(string name)
        {
            return this.FirstOrDefault(x => x.Name == name);
        }

        public new bool Add(ModuleOutput moduleOutput)
        {
            if (moduleOutput == null
                    || moduleOutput.Name == string.Empty
                    || GetModuleOutput(moduleOutput.Name) != null)
            {
                Console.WriteLine($"Output '{moduleOutput.Name}' failed to be added");

                return false;
            }

            base.Add(moduleOutput);

            var module = GetModuleOutput(moduleOutput.Name);

            return true;
        }
    }
}