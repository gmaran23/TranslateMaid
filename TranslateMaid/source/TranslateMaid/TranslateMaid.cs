using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Windows.Forms;
using TranslateMaidNS;
using System.Text;


/// <summary>The object for implementing an Add-in.</summary>
/// <seealso class='IDTExtensibility2' />
public class TranslateMaid : IDTExtensibility2, IDTCommandTarget
{
    #region private members

    private DTE2 _applicationObject;
    private AddIn _addInInstance;
    private CommandBarButton toolsMenuTryTranslateButton;
    private CommandBarButton codeWindowContextMenuTryTranslateButton;
    private Command translateCommand;
    private CommandBar menuBarCommandBar;
    private CommandBar codeWindowCommandBar;
    private Commands2 commands;
    private CommandBarControl toolsMenuControlControl;
    private CommandBarPopup toolsMenuPopup;
    private string toolsMenuName = "Tools"; 

    #endregion

    #region Wizard generated events

    /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
    public TranslateMaid()
    {
    }

    /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
    /// <param term='application'>Root object of the host application.</param>
    /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
    /// <param term='addInInst'>Object representing this Add-in.</param>
    /// <seealso class='IDTExtensibility2' />
    public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
    {
        try
        {
            _applicationObject = (DTE2)application;
            _addInInstance = (AddIn)addInInst;

            if (connectMode == ext_ConnectMode.ext_cm_UISetup)
            {
                CreateTranslateCommand();
            }

            if (connectMode == ext_ConnectMode.ext_cm_Startup)
            {
                CreateTranslateCommand();
                AssociateKeyBoardBindings();
                AddTemporaryUI();
            }
            if (connectMode == ext_ConnectMode.ext_cm_AfterStartup)
            {
                CreateTranslateCommand();
                AssociateKeyBoardBindings();
                AddTemporaryUI();
            }
        }
        catch (Exception ex)
        {
            StringBuilder messsageBoxText = new StringBuilder();
            messsageBoxText.AppendLine("TranslateMaid.TryTranslate AddIn encountered an error.");
            messsageBoxText.AppendLine();
            messsageBoxText.AppendLine("If you keep getting this error message, disable the addin permenantly by navigating to Tools-> Add-in Manager...");
            messsageBoxText.AppendLine("and unchecking the checkboxes against TryTranslate.");
            messsageBoxText.AppendLine();
            messsageBoxText.AppendLine("If you are looking for a solution instead, report it at ");
            messsageBoxText.AppendLine("http://translatemaid.codeplex.com/");
            messsageBoxText.AppendLine("or");
            messsageBoxText.AppendLine("http://renouncedthoughts.wordpress.com/contact");
            messsageBoxText.AppendLine();
            messsageBoxText.AppendLine("Error: " + ex.Message + Environment.NewLine + ex.StackTrace);
            messsageBoxText.AppendLine();
            messsageBoxText.AppendLine("Copy this error text by pressing Ctrl+C.");

            MessageBox.Show(text: messsageBoxText.ToString(), caption: "TranslateMaid.TryTranslate - Error!", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Information);
        }
    }

    /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
    /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
    /// <param term='custom'>Array of parameters that are host application specific.</param>
    /// <seealso class='IDTExtensibility2' />
    public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
    {
        try
        {
            switch (disconnectMode)
            {
                case ext_DisconnectMode.ext_dm_HostShutdown:
                    {
                        DeleteTranslateCommand();
                        break;
                    }
                case ext_DisconnectMode.ext_dm_UserClosed:
                    {
                        DeleteTemporaryUI();
                        break;
                    }
                case ext_DisconnectMode.ext_dm_UISetupComplete:
                    {
                        RemoveKeyBoardBindings();
                        break;
                    }
            }
        }
        catch { }
    }

    /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
    /// <param term='custom'>Array of parameters that are host application specific.</param>
    /// <seealso class='IDTExtensibility2' />		
    public void OnAddInsUpdate(ref Array custom)
    {
    }

    /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
    /// <param term='custom'>Array of parameters that are host application specific.</param>
    /// <seealso class='IDTExtensibility2' />
    public void OnStartupComplete(ref Array custom)
    {
    }

    /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
    /// <param term='custom'>Array of parameters that are host application specific.</param>
    /// <seealso class='IDTExtensibility2' />
    public void OnBeginShutdown(ref Array custom)
    {
    }

    /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
    /// <param term='commandName'>The name of the command to determine state for.</param>
    /// <param term='neededText'>Text that is needed for the command.</param>
    /// <param term='status'>The state of the command in the user interface.</param>
    /// <param term='commandText'>Text requested by the neededText parameter.</param>
    /// <seealso class='Exec' />
    public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
    {
        if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
        {
            if (commandName == "TranslateMaid.TryTranslate")
            {
                if (GetSelectedText() != string.Empty)
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                    return;
                }
                else
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported;
                }
            }
        }
    }

    /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
    /// <param term='commandName'>The name of the command to execute.</param>
    /// <param term='executeOption'>Describes how the command should be run.</param>
    /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
    /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
    /// <param term='handled'>Informs the caller if the command was handled or not.</param>
    /// <seealso class='Exec' />
    public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
    {
        handled = false;
        if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
        {
            if (commandName == "TranslateMaid.TryTranslate")
            {
                string selectedText = GetSelectedText();
                string selectedTextTrimmed = selectedText.Trim();
                if (selectedText == string.Empty)
                {
                    _applicationObject.StatusBar.Text = commandName + ": Invalid Operation: No text to translate.";
                }
                else
                {
                    if (string.IsNullOrEmpty(selectedTextTrimmed))
                    {
                        _applicationObject.StatusBar.Text = commandName + ": Invalid Operation: Selected is not valid for translation.";
                    }
                    else
                    {
                        if (selectedTextTrimmed.Length > 350 && selectedTextTrimmed.Length < 700)
                        {
                            _applicationObject.StatusBar.Text = commandName + ": Warning: Selected text is pretty long, hence the result may appear truncated.";
                        }
                        else if (selectedTextTrimmed.Length >= 700)
                        {
                            _applicationObject.StatusBar.Text = commandName + ": Warning: Selected text is too much to handle, hence may result in truncation or error.";
                        }
                        else
                        {
                            _applicationObject.StatusBar.Text = commandName + ": Trying to traslate: " + selectedTextTrimmed;
                        }
                        GoTryRunningTheMainModule(selectedTextTrimmed);
                        _applicationObject.StatusBar.Clear();
                    }
                }
                handled = true;
            }
        }
    } 
    
    #endregion
    
    #region private helper methods

    private void AssociateKeyBoardBindings()
    {
        Object[] bindings = null;
        int bindingNumber = 0;
        Command tCommand = _applicationObject.Commands.Item("TranslateMaid.TryTranslate", -1);

        bindings = ((System.Object[])(tCommand.Bindings));
        bindingNumber = bindings.Length;
        object[] newBinginds = new object[bindingNumber + 4];
        System.Array.Copy(bindings, newBinginds, Math.Min(bindings.Length, newBinginds.Length));
        bindings = newBinginds;

        bindings[bindingNumber] = "Global::Ctrl+`,Ctrl+`";
        bindings[bindingNumber + 1] = "Global::Ctrl+Shift+`,Ctrl+Shift+`";
        bindings[bindingNumber + 2] = "Global::Ctrl+`,`";
        bindings[bindingNumber + 3] = "Global::Ctrl+Shift+`,`";

        tCommand.Bindings = bindings;
    }

    private void RemoveKeyBoardBindings()
    {
        translateCommand = _applicationObject.Commands.Item("TranslateMaid" + "." + "TryTranslate", -1);
        Object[] keyboardBindings = new Object[0];
        translateCommand.Bindings = keyboardBindings;
    }

    private void CreateTranslateCommand()
    {
        commands = (Commands2)_applicationObject.Commands;
        // Try to retrieve the command, just in case it was already created
        try
        {
            translateCommand = _applicationObject.Commands.Item("TranslateMaid" + "." + "TryTranslate", -1);
        }
        catch
        {
            try
            {
                object[] contextGUIDS = new object[] { };
                translateCommand = commands.AddNamedCommand2(_addInInstance, "TryTranslate", "Try Translate", "Tries to translate the selected text via Microsoft Translator", false, 1, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);
            }
            catch (System.ArgumentException)
            {
                //If we are here, then the exception is probably because a command with that name
                //  already exists. 
            }
        }
    }

    private void DeleteTemporaryUI()
    {
        if ((toolsMenuTryTranslateButton != null))
        {
            toolsMenuTryTranslateButton.Delete(true);
        }
        if ((codeWindowContextMenuTryTranslateButton != null))
        {
            codeWindowContextMenuTryTranslateButton.Delete(true);
        }
    }

    private void DeleteTranslateCommand()
    {
        if ((translateCommand != null))
        {
            translateCommand.Delete();
        }
    }

    private void AddTemporaryUI()
    {
        menuBarCommandBar = ((CommandBars)_applicationObject.CommandBars)["MenuBar"];
        codeWindowCommandBar = ((CommandBars)_applicationObject.CommandBars)["Code Window"];

        toolsMenuControlControl = menuBarCommandBar.Controls[toolsMenuName];
        toolsMenuPopup = (CommandBarPopup)toolsMenuControlControl;

        //Add temporary UI
        try
        {
            if ((translateCommand != null) && (toolsMenuPopup != null))
            {
                toolsMenuTryTranslateButton = (CommandBarButton)translateCommand.AddControl(toolsMenuPopup.CommandBar, 1);
            }

            if ((translateCommand != null) && (codeWindowCommandBar != null))
            {
                codeWindowContextMenuTryTranslateButton = (CommandBarButton)translateCommand.AddControl(codeWindowCommandBar, codeWindowCommandBar.Controls.Count);
            }
        }
        catch { }
    }

    private void GoTryRunningTheMainModule(string textToTranslate)
    {
        ModalForm modalForm = new ModalForm(textToTranslate);
        InterceptMouse._hookID = InterceptMouse.SetHook(InterceptMouse._proc);
        modalForm.ShowDialog();
        InterceptMouse.UnhookWindowsHookEx(InterceptMouse._hookID);
    }

    private string GetSelectedText()
    {
        string retVal = string.Empty;
        try
        {
            TextDocument objTextDocument = (TextDocument)_applicationObject.ActiveDocument.Object("");
            TextSelection objTextSelection = objTextDocument.Selection;

            retVal = objTextSelection.Text;
        }
        catch { }
        return retVal;
    } 

    #endregion
}

