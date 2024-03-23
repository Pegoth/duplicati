// Copyright (C) 2024, The Duplicati Team
// https://duplicati.com, hello@duplicati.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Duplicati.Library.Interface
{
    /// <summary>
    /// The interface that defines a GUI for a plugable component, such as a backend or encryption module.
    /// All options are stored in a dictionary, which is persisted to the database.
    /// When creating a backup, the option dictionary will be empty, when editing, it will contain the contents from the last edit.
    /// The options in the dictionary are free form, and should contain the state of the userinterface.
    /// When the backup is invoked, the <see cref="GetConfiguration"/> method is invoked with the options dictionary.
    /// At this point the control should setup the Duplicati options, based on the saved userinterface state.
    /// This enables the user interface to be decoupled from the actual options.
    /// </summary>
    public interface IGUIControl
    {
        /// <summary>
        /// The title of the page, shown as the wizard title or the tab caption
        /// </summary>
        string PageTitle { get; }

        /// <summary>
        /// The page description, shown as a helptext in the wizard or as a tooltip on a tab
        /// </summary>
        string PageDescription { get; }

        /// <summary>
        /// A method to retrieve a user interface control.
        /// The method should keep a reference to the dictionary.
        /// The control will be resized to fit in the container.
        /// </summary>
        /// <param name="applicationSettings">The set of settings defined by the calling application</param>
        /// <param name="options">A set of options, either empty or previously stored by the control</param>
        /// <returns>The control that represents the user interface</returns>
        Control GetControl(IDictionary<string, string> applicationSettings, IDictionary<string, string> options);

        /// <summary>
        /// Method that is called when the user leaves the form.
        /// This method can be used to store data from the interface in the dictionary,
        /// so the user interface can be reconstructed later.
        /// Do not throw exceptions to interrupt this method as that will remove the "back" option in the wizard,
        /// return false in the <see cref="Validate"/> method instead.
        /// </summary>
        /// <param name="control">The UI object created by the GetControl call</param>
        void Leave(Control control);

        /// <summary>
        /// A method that is called when the form should be validated.
        /// The method should inform the user if the validation fails.
        /// </summary>
        /// <param name="control">The UI object created by the GetControl call</param>
        /// <returns>True if the data is valid, false otherwise</returns>
        bool Validate(Control control);

        /// <summary>
        /// This method sets up the Duplicati options, based on a previous setup.
        /// The method should read the saved setup from <paramref name="guiOptions"/>, and set the appropriate <paramref name="commandlineOptions"/>.
        /// If the UI represents a backend, the function should return the backend URL, otherwise the return value is ignored.
        /// </summary>
        /// <param name="applicationSettings">The set of settings defined by the calling application</param>
        /// <param name="guiOptions">A set of previously saved options for the control</param>
        /// <param name="commandlineOptions">A set of commandline options passed to Duplicati</param>
        /// <returns>The destination path if the control is for a backend, otherwise null</returns>
        string GetConfiguration(IDictionary<string, string> applicationSettings, IDictionary<string, string> guiOptions, IDictionary<string, string> commandlineOptions);
    }
}
