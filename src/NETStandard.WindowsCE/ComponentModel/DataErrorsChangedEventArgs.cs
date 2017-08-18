// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.ComponentModel
{
    /// <summary>
    /// <para>Provides data for the <see langword='ErrorsChanged'/>
    /// event.</para>
    /// </summary>
    public class DataErrorsChangedEventArgs : EventArgs
    {
        private readonly string _propertyName;

        /// <summary>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.DataErrorsChangedEventArgs'/>
        /// class.</para>
        /// </summary>
        public DataErrorsChangedEventArgs(string propertyName)
        {
            _propertyName = propertyName;
        }

        /// <summary>
        ///    <para>Indicates the name of the property whose errors changed.</para>
        /// </summary>
        public virtual string PropertyName
        {
            get
            {
                return _propertyName;
            }
        }
    }
}