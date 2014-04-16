#region Author
/************************************************************************************************************
Author: EpixCode (Keven Poulin)
Website: http://www.EpixCode.com
GitHub: https://github.com/EpixCode
Twitter: https://twitter.com/EpixCode (@EpixCode)
LinkedIn: http://www.linkedin.com/in/kevenpoulin
************************************************************************************************************/
#endregion

#region Copyright
/************************************************************************************************************
Copyright (C) 2014 EpixCode

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge, publish, distribute,
sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished 
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING 
BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
************************************************************************************************************/
#endregion

#region Class Documentation
/************************************************************************************************************
Class Name:     WeakKeyPair.cs
Namespace:      Com.EpixCode.Util.WeakReference.WeakDictionary
Type:           Util
Definition:
                [WeakKeyPair] is a [KeyValuePair] that encapsulate the key into a [WeakReference] . By doing so, 
                the key could be release from memory if it’s no longer used by any other reference. [WeakKeyPair] 
                is mostly used by [WeakDictionary] (Implementation from EpixCode).
Example:
                WeakKeyPair<object, string> myWeakKeyPair = new WeakKeyPair<object, string>(myObject, "myString");
                if(myWeakKeyPair.IsAlive)
                {
                    Debug.Log(myWeakKeyPair.Key + " = " + myWeakKeyPair.Value);
                }
                else
                {
                    Debug.Log(myWeakKeyPair + " content has been relesed from memory!");
                }
                
************************************************************************************************************/
#endregion

#region Using
using System;
using UnityEngine;
#endregion

namespace Com.EpixCode.Util.WeakReference.WeakDictionary
{
    public class WeakKeyPair<TKey, TValue> : IDisposable
    {
        /*********************************************************
        * Member Variables
        *********************************************************/
        private System.WeakReference _keyWeakRef;
        private TValue _valueRef;

        /*********************************************************
        * Accessors / Mutators
        *********************************************************/
        public TKey Key
        {
            get
            {
                return (TKey)_keyWeakRef.Target;
            }
        }

        public TValue Value
        {
            get
            {
                return _valueRef;
            }
            set
            {
                _valueRef = value;
            }
        }

        public bool IsAlive
        {
            get
            {
                return (bool)(_keyWeakRef != null && _keyWeakRef.IsAlive && !this.Key.Equals(null));
            }
        }
        
        /**********************************************************
        * Public Methods
        *********************************************************/
        public WeakKeyPair(TKey key, TValue value)
        {
            _keyWeakRef = new System.WeakReference(key);
            _valueRef = value;
        }

        public void Dispose()
        {
            if (_keyWeakRef != null && _keyWeakRef.IsAlive && _keyWeakRef.Target is IDisposable)
            {
                (_keyWeakRef.Target as IDisposable).Dispose();
            }

            if (_valueRef != null && _valueRef is IDisposable)
            {
                (_valueRef as IDisposable).Dispose();
            }

            _keyWeakRef = null;
            _valueRef = default(TValue);
        }

        /**********************************************************
        * Private Methods
        *********************************************************/
    }
}