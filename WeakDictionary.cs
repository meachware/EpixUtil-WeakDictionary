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
Class Name:     WeakDictionary.cs
Namespace:      Com.EpixCode.Util.WeakReference.WeakDictionary
Type:           Util / Collection
Definition:
                [WeakDictionary] is an alternative to [ConditionalWeakTable] (Only available in .NET Framework 4 +) 
                for Unity. It gives the ability to index a dictionary with object references that will automatically 
                be released if they are no longer used.
Example:
                WeakDictionary<object, string> myWeakDictionary = new WeakDictionary<object, string>();
                myWeakDictionary.Add(myObject, "myString");
************************************************************************************************************/
#endregion

#region Using
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#endregion

namespace Com.EpixCode.Util.WeakReference.WeakDictionary
{
    public class WeakDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IEnumerable
    {
        /*********************************************************
        * Member Variables
        *********************************************************/
        private IDictionary<int, WeakKeyPair<TKey, TValue>> _weakRefDict = new Dictionary<int, WeakKeyPair<TKey, TValue>>();
        private IDictionary<TKey, TValue> _hardRefDict = new Dictionary<TKey, TValue>();
        private bool _isAutoCean = true;
        
        /*********************************************************
        * Accessors / Mutators
        *********************************************************/
        public int Count
        {
            get
            {
                int count = _weakRefDict.Count + _hardRefDict.Count;
                return count;
            }
        }

        public bool IsReadOnly
        {
            get 
            { 
                return (_weakRefDict.IsReadOnly && _hardRefDict.IsReadOnly); 
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                ICollection<TKey> keyList = new List<TKey>();
                foreach (KeyValuePair<TKey, TValue> entry in this)
                {
                    keyList.Add(entry.Key);
                }
                return keyList;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                ICollection<TValue> valueList = new List<TValue>();
                foreach (KeyValuePair<TKey, TValue> entry in this)
                {
                    valueList.Add(entry.Value);
                }
                return valueList;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value = GetFromWeakReferenceDict(key);
                if (object.Equals(value, default(TValue)))
                {
                    value = GetFromHardReferenceDict(key);
                }
                return value;
            }

            set
            {
                if (!_hardRefDict.ContainsKey(key))
                {
                    int hashCodeKey = key.GetHashCode();
                    if (_weakRefDict.ContainsKey(hashCodeKey))
                    {
                        WeakKeyPair<TKey, TValue> weakKeyPair = _weakRefDict[hashCodeKey];
                        weakKeyPair.Value = value;
                    }
                    else
                    {
                        Debug.LogWarning("WeakDictionary - There is no entry for key [" + key + "].");
                    }
                }
                else
                {
                    _hardRefDict[key] = value;
                }
            }
        }

        public bool IsAutoClean
        {
            get
            {
                return _isAutoCean;
            }
            set
            {
                _isAutoCean = value;
            }
        }

        /**********************************************************
        * Explicit Methods
        *********************************************************/
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            return Remove(key);
        }

        /**********************************************************
        * Public Methods
        *********************************************************/
        public void Add(TKey key, TValue value)
        {
            if (!AddToWeakReferenceDict(key, value))
            {
                if (!AddToHardReferenceDict(key, value))
                {
                    Debug.LogWarning("WeakDictionary - The key [" + key + "] is already registered!");
                }
                else
                {
                    Debug.Log("WeakDictionary - HashCode collision, The key [" + key + "] is hard referenced.");
                }
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Remove(TKey key)
        {
            if (!RemoveFromWeakReferenceDict(key))
            {
                if (!RemoveFromHardReferenceDict(key))
                {
                    Debug.LogWarning("WeakDictionary - The key [" + key + "] isn't registered!");
                    return false;
                }
            }
            return true;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public bool ContainsKey(TKey key)
        {
            if (!_hardRefDict.ContainsKey(key))
            {
                int hashCodeKey = key.GetHashCode();
                if (!_weakRefDict.ContainsKey(hashCodeKey))
                {
                    return false;
                }
            }
            return true;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key);
        }

        public void Clear()
        {
            _weakRefDict.Clear();
            _hardRefDict.Clear();
        }

        public bool Clean()
        {
            List<int> weakKeyListToRemove = new List<int>();
            List<KeyValuePair<TKey, TValue>> hardEntryListToRemove = new List<KeyValuePair<TKey, TValue>>();
            bool success = false;

            foreach (KeyValuePair<int, WeakKeyPair<TKey, TValue>> entry in _weakRefDict)
            {
                if (!entry.Value.IsAlive)
                {
                    weakKeyListToRemove.Add(entry.Key);
                }
            }

            foreach (KeyValuePair<TKey, TValue> entry in _hardRefDict)
            {
                if (entry.Key.Equals(null))
                {
                    hardEntryListToRemove.Add(entry);
                }
            }

            for (int i = 0; i < weakKeyListToRemove.Count; i++)
            {
                Clean(weakKeyListToRemove[i]);
                success = true;
            }

            for (int i = 0; i < hardEntryListToRemove.Count; i++)
            {
                KeyValuePair<TKey, TValue> entry = hardEntryListToRemove[i];
                if (entry.Value != null && entry.Value is IDisposable)
                {
                    (entry.Value as IDisposable).Dispose();
                }
                _hardRefDict.Remove(entry);
                success = true;
            }

            return success;
        }

        public bool Clean(TKey key)
        {
            return Clean(key.GetHashCode());
        }

        private bool Clean(int hashCodeKey)
        {
            bool success = false;

            if (_weakRefDict.ContainsKey(hashCodeKey))
            {
                WeakKeyPair<TKey, TValue> weakKeyPair = _weakRefDict[hashCodeKey];
                if (!weakKeyPair.IsAlive)
                {
                    weakKeyPair.Dispose();
                    _weakRefDict.Remove(hashCodeKey);
                    success = true;
                }
            }

            return success;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (ContainsKey(key))
            {
                value = this[key];
                return true;
            }

            value = default(TValue);
            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            foreach (KeyValuePair<int, WeakKeyPair<TKey, TValue>> entry in _weakRefDict)
            {
                KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(entry.Value.Key, entry.Value.Value);
                array.SetValue(keyValuePair, index);
                index = index + 1;
            }

            foreach (KeyValuePair<TKey, TValue> entry in _hardRefDict)
            {
                array.SetValue(entry, index);
                index = index + 1;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (IsAutoClean)
            {
                Clean();
            }

            foreach (KeyValuePair<int, WeakKeyPair<TKey, TValue>> entry in _weakRefDict)
            {
                yield return new KeyValuePair<TKey, TValue>(entry.Value.Key, entry.Value.Value);
            }

            foreach (KeyValuePair<TKey, TValue> entry in _hardRefDict)
            {
                yield return entry;
            }
        }

        /**********************************************************
        * Private Methods
        *********************************************************/
        private bool AddToWeakReferenceDict(TKey key, TValue value)
        {
            int hashCodeKey = key.GetHashCode();

            if (IsAutoClean)
            {
                Clean(hashCodeKey);
            }

            if (!_weakRefDict.ContainsKey(hashCodeKey))
            {
                WeakKeyPair<TKey, TValue> weakPair = new WeakKeyPair<TKey, TValue>(key, value);
                _weakRefDict.Add(hashCodeKey, weakPair);
                return true;
            }
            else
            {
                if (_weakRefDict[hashCodeKey].Key.Equals(key))
                {
                    return true;
                }
            }

            return false;
        }

        private bool AddToHardReferenceDict(TKey key, TValue value)
        {
            if (!_hardRefDict.ContainsKey(key))
            {
                _hardRefDict.Add(key, value);
                return true;
            }
            else
            {
                if (_hardRefDict[key].Equals(value))
                {
                    return true;
                }
            }

            return false;
        }

        private bool RemoveFromWeakReferenceDict(TKey key)
        {
            return RemoveFromWeakReferenceDict(key.GetHashCode());
        }

        private bool RemoveFromWeakReferenceDict(int hashCodeKey)
        {
            if (_weakRefDict.ContainsKey(hashCodeKey))
            {
                _weakRefDict.Remove(hashCodeKey);
                return true;
            }

            return false;
        }

        private bool RemoveFromHardReferenceDict(TKey key)
        {
            if (_hardRefDict.ContainsKey(key))
            {
                _hardRefDict.Remove(key);
                return true;
            }

            return false;
        }

        private TValue GetFromWeakReferenceDict(TKey key)
        {
            int hashCodeKey = key.GetHashCode();

            if (IsAutoClean)
            {
                Clean(hashCodeKey);
            }

            if (_weakRefDict.ContainsKey(hashCodeKey))
            {
                return _weakRefDict[hashCodeKey].Value;
            }

            return default(TValue);
        }

        private TValue GetFromHardReferenceDict(TKey key)
        {
            if (_hardRefDict.ContainsKey(key))
            {
                return _hardRefDict[key];
            }

            return default(TValue);
        }
    }
}