﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace z80_pla_checker
{
    public partial class FormMain : Form
    {
        /// <summary>
        /// Master PLA table
        /// </summary>
        private readonly ClassPla pla = new ClassPla();
        /// <summary>
        /// Current modifiers
        /// </summary>
        private ClassPlaEntry.Modifier modifier = ClassPlaEntry.Modifier.IXY0;


        private ClassPlaEntry.Prefix prefix = ClassPlaEntry.Prefix.XX;
        private readonly List<ClassOpTable> opTable = new List<ClassOpTable>();
        private readonly List<string> commands = new List<string>();
        private int commandsBrowseIndex;

        public FormMain()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
        }

        public static string GetAbsolutePath(string filename)
        {
            string dir = Path.GetDirectoryName(Application.StartupPath);
            return Path.Combine(dir, filename);
        }

        /// <summary>
        /// This is the main program's startup function
        /// </summary>
        public void OnStart()
        {
            ClassLog.Log("PLA Checker Tool  Copyright (C) 2014  Goran Devic");
            ClassLog.Log("This program comes with ABSOLUTELY NO WARRANTY");
            ClassLog.Log("This is free software and you are welcome to redistribute it under certain conditions; for details see GPLv3 license.");
            ClassLog.Log("---------------------------------------------------------------------------------------------------------------------");

            // Load the PLA table from a text file.
            String plaFile = Properties.Settings.Default.plaFileName;
            if (!pla.Load(plaFile))
            {
                ClassLog.Log("*** Error loading the master input PLA source table ***");
                return;
            }

            //// Create 3 opcode tables, one for each class of instructions
            //opTable.Add(new ClassOpTable());
            //opTable.Add(new ClassOpTable());
            //opTable.Add(new ClassOpTable());
            //opTable[(int)ClassPlaEntry.Prefix.XX].Load(GetAbsolutePath(@"..\..\..\resources\opcodes-xx.txt"));
            //opTable[(int)ClassPlaEntry.Prefix.CB].Load(GetAbsolutePath(@"..\..\..\resources\opcodes-cb.txt"));
            //opTable[(int)ClassPlaEntry.Prefix.ED].Load(GetAbsolutePath(@"..\..\..\resources\opcodes-ed.txt"));

            ClassLog.Log(Command("?"));
        }

        /// <summary>
        /// Print out a log message & always show the last line
        /// </summary>
        public void Log(String s)
        {
            logText.AppendText(s + Environment.NewLine);
            logText.SelectionStart = logText.Text.Length;
            logText.ScrollToCaret();
        }

        /// <summary>
        /// Clear the log text panel
        /// </summary>
        private void BtClearClick(object sender, EventArgs e)
        {
            logText.Clear();
        }

        /// <summary>
        /// Exit the application
        /// </summary>
        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Close();
            Properties.Settings.Default.Save();
        }

        private static int ScanNumber(string arg, int baseValue)
        {
            try
            {
                return Convert.ToInt32(arg, baseValue);
            }
            catch(Exception ex)
            {
                ClassLog.Log(ex.Message + ": " + arg);
                return -1;
            }
        }

        /// <summary>
        /// List all PLA entries that trigger on a specific opcode
        /// If the opcode # (hex) was not given, dump that information for all opcodes (0-FF)
        /// </summary>
        private void MatchPLA(string arg)
        {
            int op = -1;
            if (!string.IsNullOrEmpty(arg))
            {
                op = ScanNumber(arg, 16);
                if (op < 0)
                    return;
            }
            for (int x = 0; x < 256; x++)
            {
                if (op >= 0 && x != op)
                    continue;
                ClassLog.Log(String.Format("Opcode: {0} {1:X02} ", prefix, x));

                Byte opcode = Convert.ToByte(x);
                List<string> m = pla.TableMatch(opcode, modifier, prefix);

                foreach (var s in m)
                    ClassLog.Log(s);
            }
        }

        /// <summary>
        /// List all opcodes that trigger on a given PLA table index
        /// </summary>
        private void MatchOpcodes(ClassPlaEntry.Modifier modifier, string arg)
        {
            int index = ScanNumber(arg, 10);
            if (index < 0)
                return;
            List<string> m = pla.MatchPLA(modifier, index);
            if (m.Count == 0)
                return;
            ClassLog.Log(String.Format("PLA Entry: {0}  Modifier: {1}  Prefix: {2}", index, modifier, prefix));
            foreach (var s in m)
                ClassLog.Log(s);
        }

        /// <summary>
        /// User clicked on the Redo button: repeat the command
        /// </summary>
        private void BtRedoClick(object sender, EventArgs e)
        {
            ClassLog.Log(string.Format("{0}>>> {1}", commands.Count, commands[commands.Count - 1]));
            string response = Command(commands[commands.Count - 1]);
            if (!string.IsNullOrEmpty(response))
                ClassLog.Log(response);
        }

        /// <summary>
        /// Update button state after the internal flag state change
        /// </summary>
        private void UpdateButtons()
        {
            btIX0.Checked = (modifier & ClassPlaEntry.Modifier.IXY0) != 0;
            btIX1.Checked = (modifier & ClassPlaEntry.Modifier.IXY1) != 0;
            btHALT.Checked = (modifier & ClassPlaEntry.Modifier.HALT) != 0;
            btALU.Checked = (modifier & ClassPlaEntry.Modifier.ALU) != 0;
            btXX.Checked = (modifier & ClassPlaEntry.Modifier.XX) != 0;
            btCB.Checked = (modifier & ClassPlaEntry.Modifier.CB) != 0;
            btED.Checked = (modifier & ClassPlaEntry.Modifier.ED) != 0;

            ClassLog.Log("Set modifier to " + modifier);
        }

        private void BtIx0Click(object sender, EventArgs e)
        {
            if ((modifier & ClassPlaEntry.Modifier.IXY0) != 0)
                modifier &= ~ClassPlaEntry.Modifier.IXY0;
            else
            {
                modifier |= ClassPlaEntry.Modifier.IXY0;
                modifier &= ~ClassPlaEntry.Modifier.IXY1;                
            }
            UpdateButtons();
        }

        private void BtIx1Click(object sender, EventArgs e)
        {
            if ((modifier & ClassPlaEntry.Modifier.IXY1) != 0)
                modifier &= ~ClassPlaEntry.Modifier.IXY1;
            else
            {
                modifier |= ClassPlaEntry.Modifier.IXY1;
                modifier &= ~ClassPlaEntry.Modifier.IXY0;
            }
            UpdateButtons();
        }

        private void BtHaltClick(object sender, EventArgs e)
        {
            modifier ^= ClassPlaEntry.Modifier.HALT;
            UpdateButtons();
        }

        private void BtAluClick(object sender, EventArgs e)
        {
            modifier ^= ClassPlaEntry.Modifier.ALU;
            UpdateButtons();
        }

        private void BtXxClick(object sender, EventArgs e)
        {
            if ((modifier & ClassPlaEntry.Modifier.XX) != 0)
                modifier &= ~ClassPlaEntry.Modifier.XX;
            else
            {
                modifier |= ClassPlaEntry.Modifier.XX;
                modifier &= ~(ClassPlaEntry.Modifier.CB | ClassPlaEntry.Modifier.ED);
            }
            UpdateButtons();
        }

        private void BtCbClick(object sender, EventArgs e)
        {
            if ((modifier & ClassPlaEntry.Modifier.CB) != 0)
                modifier &= ~ClassPlaEntry.Modifier.CB;
            else
            {
                modifier |= ClassPlaEntry.Modifier.CB;
                modifier &= ~(ClassPlaEntry.Modifier.XX | ClassPlaEntry.Modifier.ED);
            }
            UpdateButtons();
        }

        private void BtEdClick(object sender, EventArgs e)
        {
            if ((modifier & ClassPlaEntry.Modifier.ED) != 0)
                modifier &= ~ClassPlaEntry.Modifier.ED;
            else
            {
                modifier |= ClassPlaEntry.Modifier.ED;
                modifier &= ~(ClassPlaEntry.Modifier.XX | ClassPlaEntry.Modifier.CB);
            }
            UpdateButtons();
        }

        /// <summary>
        /// Implements a simple command history
        /// </summary>
        private void TextOpKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Mark the handled flag so this key won't be processed.
                e.Handled = true;
                string cmd = textOp.Text.Trim();
                if (cmd.Length > 0)
                {
                    commands.Add(cmd);
                    btRedo.Enabled = true;
                    ClassLog.Log(string.Format("{0}>>> {1}", commands.Count, cmd));
                    string response = Command(textOp.Text);
                    if (!string.IsNullOrEmpty(response))
                        ClassLog.Log(response);

                    commandsBrowseIndex = commands.Count;
                    textOp.Text = "";
                }
                textOp.Focus();
            }
            if (e.KeyCode == Keys.PageUp && commandsBrowseIndex > 0)
            {
                commandsBrowseIndex--;
                textOp.Text = commands[commandsBrowseIndex];
                e.Handled = true;
            }
            if (e.KeyCode == Keys.PageDown && commandsBrowseIndex < commands.Count - 1)
            {
                commandsBrowseIndex++;
                textOp.Text = commands[commandsBrowseIndex];
                e.Handled = true;
            }
            if (e.KeyCode == Keys.Escape)
            {
                textOp.Text = "";
                e.Handled = true;
            }
        }

        /// <summary>
        /// Execute a command
        /// </summary>
        private string Command(string cmd)
        {
            try
            {
                string[] tokens = cmd.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 0)
                    return "";
                switch (tokens[0])
                {
                    case "?":
                    case "h":
                        return Environment.NewLine +
                            "p         - Dump the content of the PLA table" + Environment.NewLine +
                            "p [#]     - For a given PLA entry # (dec) show opcodes that trigger it" + Environment.NewLine +
                            "m [#]     - Match opcode # (hex) with a PLA entry (or match 0-FF)" + Environment.NewLine +
                            "gen       - Generate a Verilog PLA module" + Environment.NewLine +
                            "gens      - Generate a Verilog PLA module sorted by the ordinal PLA number" + Environment.NewLine +
                            "test [#]  - Dump opcode table in various ways:" + Environment.NewLine +
                            "            test 3  ..suitable to compare to the RTL simulation of decode/execute dump" + Environment.NewLine +
                            "cls       - Clear the screen";
                    case "p": if (tokens.Length > 1)
                            MatchOpcodes(modifier, tokens[1]);
                        else
                            pla.Dump();
                        break;
                    case "m": MatchPLA(tokens.Length > 1 ? tokens[1] : "");
                        break;
                    case "gen": pla.GenVerilogPla(false);
                        break;
                    case "gens": pla.GenVerilogPla(true);
                        break;
                    case "cls": BtClearClick(null, null);
                        break;
                    case "test":
                        {
                            int testnum = 0;
                            if (tokens.Length > 1)
                                testnum = ScanNumber(tokens[1], 10);
                            // Test #3 is special: we loop over all combination of prefix and opcode
                            // to generate a test file suitable to compare to the RTL simulation
                            if (testnum == 3)
                            {
                                using (StreamWriter w = File.CreateText("dumps.txt"))
                                {
                                    pla.Test(ClassPlaEntry.Modifier.IXY0, ClassPlaEntry.Prefix.XX, 3, w);
                                    pla.Test(ClassPlaEntry.Modifier.IXY1, ClassPlaEntry.Prefix.XX, 3, w);
                                    pla.Test(ClassPlaEntry.Modifier.IXY0, ClassPlaEntry.Prefix.CB, 3, w);
                                    pla.Test(ClassPlaEntry.Modifier.IXY1, ClassPlaEntry.Prefix.CB, 3, w);
                                    pla.Test(ClassPlaEntry.Modifier.IXY0, ClassPlaEntry.Prefix.ED, 3, w);
                                    pla.Test(ClassPlaEntry.Modifier.IXY1, ClassPlaEntry.Prefix.ED, 3, w);
                                }
                            }
                            else
                            {
                                pla.Test(modifier, prefix, testnum);
                                opTable[prefix == ClassPlaEntry.Prefix.All ? 0 : (int)prefix].Dump();                                
                            }
                        }
                        break;
                    default:
                        return "?";
                }
            }
            catch (Exception ex)
            {
                ClassLog.Log("Error: " + ex.Message);
            }
            return string.Empty;
        }

        /// <summary>
        /// Select the input PLA table file to load
        /// </summary>
        private void LoadPlaTable(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Title = "Select a PLA table source file";
            dlg.Filter = @"z80-pla.txt|*.txt|All files|*.*";
            dlg.FileName = Properties.Settings.Default.plaFileName;
            if (dlg.ShowDialog() == DialogResult.OK)
                Properties.Settings.Default.plaFileName = dlg.FileName;
        }
    }
}
