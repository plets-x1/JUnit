using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lesse.Core.ControlAndConversionStructures;
using Lesse.Core.Interfaces;
using Lesse.Modeling.TestPlanStructure;

namespace Lesse.JUnit {
    public class JUnitDriverGenerator : ScriptGenerator {
        private int testCaseCount = 0;

        public void GenerateScript (List<GeneralUseStructure> listPlanStructure, String path) {
            GenerateJUnitFromTestPlan (listPlanStructure, path);
        }

        private void GenerateJUnitFromTestPlan (List<GeneralUseStructure> listPlanStructure, String path) {
            //path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\GeneratedTest.java";

            foreach (GeneralUseStructure planStructure in listPlanStructure) {
                TestPlanForTCC testPlan = (TestPlanForTCC) planStructure;
                StreamWriter sw = new StreamWriter (path + @"\YourClassTest.java");

                WriteImportsAndConstructor (sw);
                //sw.WriteLine("import org.junit.Before;");

                for (int i = 0; i < testPlan.TestCases.Count; i++) {
                    TestCaseForTCC testCase = testPlan.TestCases[i];
                    int maxTestCaseQuantity = 0;

                    List<TestStepForTCC> tsWithParameters = (from ts in testCase.TestSteps where ts.Input.Contains (',') select ts).ToList ();

                    foreach (TestStepForTCC ts in tsWithParameters) {
                        int testCaseQuantity = ts.Input.Split (',').Count ();

                        if (testCaseQuantity > maxTestCaseQuantity) {
                            maxTestCaseQuantity = testCaseQuantity;
                        }
                    }

                    List<TestStepForTCC> constructors = (from ts in testCase.TestSteps where ts.ActionType.Equals ("0") select ts).ToList ();

                    for (int j = 0; j < maxTestCaseQuantity; j++) {
                        WriteTestCaseHeader (sw);

                        foreach (TestStepForTCC constructor in constructors) {
                            String objectName = constructor.Receiver.ToLower ();
                            sw.WriteLine ("\t\t" + constructor.Receiver + " " + objectName + " = new " + constructor.Receiver + "();");
                        }

                        for (int k = 0; k < testCase.TestSteps.Count; k++) {
                            TestStepForTCC step = testCase.TestSteps[k];
                            if (step.ActionType.Equals ("2") || step.ActionType.Equals ("0")) {
                                continue;
                            } else {
                                //Método sem parâmetros
                                if (String.IsNullOrEmpty (step.Input)) {
                                    sw.WriteLine ("\t\t" + step.Receiver.ToLower () + "." + step.Method + "();");
                                }
                                //Testando com apenas um conjunto de parâmetros
                                else if (!step.Input.Contains (',')) {
                                    String[] singleEntryParams = step.Input.Split (';');
                                    foreach (String singleEntryParam in singleEntryParams) {
                                        String singleEntryParamAux = singleEntryParam;

                                        if (singleEntryParam.Contains ('{')) {
                                            singleEntryParamAux = singleEntryParamAux.Substring (1);
                                        }
                                        if (singleEntryParam.Contains ('}')) {
                                            singleEntryParamAux = singleEntryParamAux.Substring (0, singleEntryParam.Length - 1);
                                        }
                                        //TO DO
                                    }
                                }
                                //Testando com diversos conjuntos de parâmetros
                                else {
                                    MultipleInputData (sw, step, j);
                                }
                            }
                        }
                        sw.WriteLine ("\t}");
                    }
                }
                sw.WriteLine ("}");
                sw.Close ();
            }
        }

        private void WriteImportsAndConstructor (StreamWriter sw) {
            sw.WriteLine ("import org.junit.Test;");
            sw.WriteLine ("import static org.junit.Assert.*;");
            sw.WriteLine ();
            sw.WriteLine ("public class YourClassTest");
            sw.WriteLine ("{");
            sw.WriteLine ("\tpublic YourClassTest()");
            sw.WriteLine ("\t{");
            sw.WriteLine ();
            sw.WriteLine ("\t}");
            sw.WriteLine ();
        }

        private void MultipleInputData (StreamWriter sw, TestStepForTCC step, int j) {
            String[] stepMethodEntries = step.Input.Split (',');
            String[] stepMethodOutputs = step.Output.Split (',');

            //for (int j = 0; j < stepMethodEntries.Count(); j++)
            {
                String[] singleEntryParams = stepMethodEntries[j].Split (';');
                String stepMethodSingleOutput = stepMethodOutputs[j];
                String lineToWrite = "\t\t" + "assertEquals(" + step.Receiver.ToLower () + "." + step.Method + "(";

                foreach (String singleEntryParam in singleEntryParams) {
                    String singleEntryParamAux = singleEntryParam;

                    if (singleEntryParam.Contains ('{')) {
                        singleEntryParamAux = singleEntryParamAux.Substring (1);
                    }
                    if (singleEntryParam.Contains ('}')) {
                        singleEntryParamAux = singleEntryParamAux.Substring (0, singleEntryParamAux.Length - 1);
                    }
                    lineToWrite += singleEntryParamAux + ",";
                }

                if (stepMethodSingleOutput.Contains ('{')) {
                    stepMethodSingleOutput = stepMethodSingleOutput.Substring (1);
                }
                if (stepMethodSingleOutput.Contains ('}')) {
                    stepMethodSingleOutput = stepMethodSingleOutput.Substring (0, stepMethodSingleOutput.Length - 1);
                }

                lineToWrite = lineToWrite.Substring (0, lineToWrite.Length - 1);
                lineToWrite += ")," + stepMethodSingleOutput + ");";
                sw.WriteLine (lineToWrite);
            }
        }

        private void WriteTestCaseHeader (StreamWriter sw) {
            testCaseCount++;
            sw.WriteLine ();
            sw.WriteLine ("\t@Test");
            sw.WriteLine ("\tpublic void testCase" + testCaseCount.ToString () + "()");
            sw.WriteLine ("\t{");
        }
    }
}