using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Lab21_AaDS
{
    class NotADictionary<K, T> : IDictionary<K, T> where K : IComparable<K>
    {
        private RedBlackTreeNode<K, T> root;
        private int size = 0;

        public T this[K key] {
            get
            {
                var node = GetNodeByKey(key);
                if (node == null)
                    throw new KeyNotFoundException();
                return node.value;
            }
            set
            {
                var node = GetNodeByKey(key);
                if (node == null)
                    throw new KeyNotFoundException();

                node.value = value;
            }
        }

        public ICollection<K> Keys
        {
            get
            {
                List<K> res = new List<K>(size);
                foreach (var node in this)
                    res.Add(node.Key);

                return res;
            }
        }

        public ICollection<T> Values
        {
            get
            {
                List<T> res = new List<T>(size);
                foreach (var node in this)
                    res.Add(node.Value);

                return res;
            }
        }

        public int Count => size;

        public bool IsReadOnly => false;

        public void Add(K key, T value)
        {
            var newNode = new RedBlackTreeNode<K, T>(key, value);

            if (root == null)
            {
                root = newNode;
                root.color = NodeColor.Black;
                size++;
                return;
            }

            newNode.color = NodeColor.Red;

            var parentForNewNode = root;
            bool gotoLeft = key.CompareTo(parentForNewNode.Key) < 0;
            var nextNode = gotoLeft ? parentForNewNode.LeftChild : parentForNewNode.RightChild;
            while (nextNode != null)
            {
                if (parentForNewNode.Key.Equals(key))
                    throw new ArgumentException("Key already exists");

                parentForNewNode = nextNode;
                gotoLeft = key.CompareTo(parentForNewNode.Key) < 0;
                nextNode = gotoLeft ? parentForNewNode.LeftChild : parentForNewNode.RightChild;
            }

            if (gotoLeft)
                parentForNewNode.SetAndLinkLeftChild(newNode);
            else
                parentForNewNode.SetAndLinkRightChild(newNode);

            RecoverAfterAdding(newNode);

            size++;
        }

        public void Add(KeyValuePair<K, T> item) => Add(item.Key, item.Value);

        public void Clear()
        {
            root = null;
            size = 0;
        }  

        public bool Contains(KeyValuePair<K, T> item)
        {
            var node = GetNodeByKey(item.Key);
            if (node == null || !node.value.Equals(item.Value))
                return false;

            return true;
        }

        public bool ContainsKey(K key)
        {
            return GetNodeByKey(key) != null;
        }

        public void CopyTo(KeyValuePair<K, T>[] array, int arrayIndex)
        {
            // well, its not a part of the task
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<K, T>> GetEnumerator() => new NotADictionaryEnumerator<K, T>(root);

        public bool Remove(K key)
        {
            var node = GetNodeByKey(key);
            if (node == null)
                return false;

            if (node.LeftChild==null)
            {
                if (node.IsLeftChild)
                    node.Parent.SetAndLinkLeftChild(node.RightChild);
                else
                    node.Parent.SetAndLinkRightChild(node.RightChild);


                return true;
            }
            else if (node.RightChild==null)
            {
                if (node.IsLeftChild)
                    node.Parent.SetAndLinkLeftChild(node.LeftChild);
                else
                    node.Parent.SetAndLinkRightChild(node.LeftChild);

                return true;
            }

            // removing it the hard way

            var lowestRightValNode = node.RightChild;               // lowest in the right subtree
            while (lowestRightValNode.LeftChild != null)
                lowestRightValNode = lowestRightValNode.LeftChild;


            if (lowestRightValNode != node.RightChild)
                lowestRightValNode.Parent?.SetAndLinkLeftChild(lowestRightValNode.RightChild);


            if (node.IsLeftChild)
                node.Parent?.SetAndLinkLeftChild(lowestRightValNode);
            else if (node.IsRightChild)
                node.Parent?.SetAndLinkRightChild(lowestRightValNode);
            else
            {
                root = lowestRightValNode;
                lowestRightValNode.Parent = null;
            }

            node.Unlink();

            lowestRightValNode.SetAndLinkLeftChild(node.LeftChild);
            if (lowestRightValNode!=node.RightChild)
                lowestRightValNode.SetAndLinkRightChild(node.RightChild);

            size--;

            if (size > 1 && lowestRightValNode.color == NodeColor.Black)
            {
                RecoverAfterRemovingButItIsNullNodeParent(lowestRightValNode, true);
            }

            return true;
        }

        public bool Remove(KeyValuePair<K, T> item)
        {
            var node = GetNodeByKey(item.Key);
            if (node==null)
                return false;

            if (!node.value.Equals(item.Value))
                return false;

            return Remove(item.Key);
        }

        public bool TryGetValue(K key, out T value)
        {
            var node = GetNodeByKey(key);
            if (node!=null)
            {
                value = node.value;
                return true;
            }

            value = default;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private RedBlackTreeNode<K, T> GetNodeByKey(K key)
        {
            var node = root;
            while (node != null)
            {
                if (node.Key.Equals(key))
                    return node;

                if (node.Key.CompareTo(key) < 0)
                    node = node.RightChild;
                else
                    node = node.LeftChild;
            }

            return null;
        }

        private void RecoverAfterAdding(RedBlackTreeNode<K, T> newNode)
        {
            if (newNode == null)
                throw new ArgumentNullException();

            while (newNode.Parent?.color == NodeColor.Red)
            {
                if (newNode.Parent.IsLeftChild)
                {
                    var grandParent = newNode.Parent.Parent;                        // definitely not null
                    if (grandParent.RightChild?.color == NodeColor.Red)
                    {
                        grandParent.LeftChild.color = NodeColor.Black;

                        if (grandParent.RightChild != null)
                            grandParent.RightChild.color = NodeColor.Black;

                        grandParent.color = NodeColor.Red;
                        newNode = grandParent;
                    }
                    else if (newNode.IsRightChild)
                    {
                        newNode = newNode.Parent;
                        RotLeft(newNode);
                    }
                    else
                    {
                        newNode.Parent.color = NodeColor.Black;
                        grandParent.color = NodeColor.Red;
                        RotRight(grandParent);
                    }
                }
                else if (newNode.Parent.IsRightChild)
                {
                    var grandParent = newNode.Parent.Parent;                        // definitely not null
                    if (grandParent.LeftChild?.color == NodeColor.Red)
                    {
                        if (grandParent.LeftChild != null)
                            grandParent.LeftChild.color = NodeColor.Black;

                        grandParent.RightChild.color = NodeColor.Black;
                        grandParent.color = NodeColor.Red;
                        newNode = grandParent;
                    }
                    else if (newNode.IsLeftChild)
                    {
                        newNode = newNode.Parent;
                        RotRight(newNode);
                    }
                    else
                    {
                        newNode.Parent.color = NodeColor.Black;
                        grandParent.color = NodeColor.Red;
                        RotLeft(grandParent);
                    }
                }

                else
                    throw new Exception("iv messed up");            // this shouldnt happen normally
            }

            root.color = NodeColor.Black;
        }

        private void RecoverAfterRemovingButItIsNullNodeParent(RedBlackTreeNode<K,T> parentNode, bool useRightNode)
        {
            // it will give control to normal recover method after curNode becomes not null
            // that makes x4 of similar code parts

            RedBlackTreeNode<K, T> bro = null;
            while ((useRightNode && parentNode.RightChild != null) || parentNode.LeftChild!=null)
            {
                if (!useRightNode)
                {
                    bro ??= parentNode.RightChild;
                    if (bro.color == NodeColor.Red)
                    {
                        bro.color = NodeColor.Black;
                        parentNode.color = NodeColor.Red;

                        RotLeft(parentNode);
                        bro = parentNode.RightChild;
                    }
                    else if ((bro.LeftChild == null || bro.LeftChild.color == NodeColor.Black) && (bro.RightChild == null || bro.RightChild.color == NodeColor.Black))
                    {
                        bro.color = NodeColor.Red;
                        RecoverAfterRemoving(parentNode);           // yeah... spaghetti. too little time to rewrite
                        return;              
                    }
                    else if (bro.RightChild == null || bro.RightChild.color == NodeColor.Black)
                    {
                        bro.LeftChild.color = NodeColor.Black;

                        bro.color = NodeColor.Red;
                        RotRight(bro);
                        bro = parentNode.RightChild;
                    }
                    else
                    {
                        bro.color = parentNode.color;
                        parentNode.color = NodeColor.Black;
                        if (bro.RightChild != null)
                            bro.RightChild.color = NodeColor.Black;
                        RotLeft(parentNode);

                        RecoverAfterRemoving(root);
                        return;
                    }
                }

                else
                {
                    bro ??= parentNode.LeftChild;
                    if (bro.color == NodeColor.Red)
                    {
                        bro.color = NodeColor.Black;
                        parentNode.color = NodeColor.Red;

                        RotRight(parentNode);
                        bro = parentNode.LeftChild;
                    }
                    else if ((bro.LeftChild == null || bro.LeftChild.color == NodeColor.Black) && (bro.RightChild == null || bro.RightChild.color == NodeColor.Black))
                    {
                        bro.color = NodeColor.Red;
                        RecoverAfterRemoving(parentNode);
                        return;
                    }
                    else if (bro.LeftChild == null || bro.LeftChild.color == NodeColor.Black)
                    {
                        bro.RightChild.color = NodeColor.Black;
                        bro.color = NodeColor.Red;
                        RotLeft(bro);
                        bro = parentNode.LeftChild;
                    }
                    else
                    {
                        bro.color = parentNode.color;
                        parentNode.color = NodeColor.Black;
                        if (bro.LeftChild != null)
                            bro.LeftChild.color = NodeColor.Black;
                        RotRight(parentNode);

                        RecoverAfterRemoving(root);
                        return;
                    }
                }
            }

            RecoverAfterRemoving(useRightNode ? parentNode.RightChild : parentNode.LeftChild);
        }

        private void RecoverAfterRemoving(RedBlackTreeNode<K,T> node)
        {
            RedBlackTreeNode<K, T> bro = null;
            bool mbRecovered = false;
            while (node!=root || node.color!=NodeColor.Black)
            {
                if (node.IsLeftChild)
                {
                    bro ??= node.Parent.RightChild;
                    if (bro.color == NodeColor.Red)
                    {
                        bro.color = NodeColor.Black;
                        var parent = node.Parent;
                        parent.color = NodeColor.Red;
                        
                        RotLeft(parent);
                        bro = node.Parent.RightChild;

                        mbRecovered = false;
                    }
                    else if ((bro.LeftChild==null || bro.LeftChild.color == NodeColor.Black) && (bro.RightChild == null || bro.RightChild.color == NodeColor.Black))
                    {
                        bro.color = NodeColor.Red;
                        node = node.Parent;

                        mbRecovered = false;
                    }
                    else if (bro.RightChild == null || bro.RightChild.color == NodeColor.Black)
                    {
                        bro.LeftChild.color = NodeColor.Black;

                        bro.color = NodeColor.Red;
                        RotRight(bro);
                        bro = node.Parent.RightChild;

                        mbRecovered = false;
                    }
                    else
                    {
                        if (mbRecovered)
                        {
                            node = root;
                            node.color = NodeColor.Black;
                            break;
                        }
                        mbRecovered = true;

                        bro.color = node.Parent.color;
                        node.Parent.color = NodeColor.Black;
                        if (bro.RightChild != null)
                            bro.RightChild.color = NodeColor.Black;
                        RotLeft(node.Parent);
                    }
                }

                // could have compacted it, but.. it works fine the way it is. And compacted version is a bit harder to read
                else
                {
                    bro ??= node.Parent.LeftChild;
                    if (bro.color == NodeColor.Red)
                    {
                        bro.color = NodeColor.Black;
                        var parent = node.Parent;
                        parent.color = NodeColor.Red;

                        RotRight(parent);
                        bro = node.Parent.LeftChild;

                        mbRecovered = false;
                    }
                    else if ((bro.LeftChild == null || bro.LeftChild.color == NodeColor.Black) && (bro.RightChild == null || bro.RightChild.color == NodeColor.Black))
                    {
                        bro.color = NodeColor.Red;
                        node = node.Parent;
                        mbRecovered = false;
                    }
                    else if (bro.LeftChild == null || bro.LeftChild.color == NodeColor.Black)
                    {
                        bro.RightChild.color = NodeColor.Black;
                        bro.color = NodeColor.Red;
                        RotLeft(bro);
                        bro = node.Parent.LeftChild;

                        mbRecovered = false;
                    }
                    else
                    {
                        if (mbRecovered)
                        {
                            node = root;
                            node.color = NodeColor.Black;
                            break;
                        }
                        mbRecovered = true;


                        bro.color = node.Parent.color;
                        node.Parent.color = NodeColor.Black;
                        if (bro.LeftChild != null)
                            bro.LeftChild.color = NodeColor.Black;
                        RotRight(node.Parent);
                    }
                }
            }
        }

        private void RotLeft(RedBlackTreeNode<K, T> node)             
        {
            if (node.RightChild == null)            // this is private method, so whaever
                throw new Exception();

            var toRightAndLeftChild = node.RightChild.LeftChild;
            var parent = node.Parent;


            if (parent != null)
            {
                if (parent.LeftChild == node)
                    parent.SetAndLinkLeftChild(node.RightChild);
                else
                    parent.SetAndLinkRightChild(node.RightChild);
            }
            else
            {       // if this is root
                root = node.RightChild;
                node.RightChild.Parent = null;
            }

            node.RightChild.SetAndLinkLeftChild(node);
            node.SetAndLinkRightChild(toRightAndLeftChild);
        }

        private void RotRight(RedBlackTreeNode<K, T> node)
        {
            if (node.LeftChild == null)            
                throw new Exception();

            var toLeftAndRightChild = node.LeftChild.RightChild;
            var parent = node.Parent;

            if (parent != null)
            {
                if (parent.LeftChild == node)
                    parent.SetAndLinkLeftChild(node.LeftChild);
                else
                    parent.SetAndLinkRightChild(node.LeftChild);
            }
            else
            {
                root = node.LeftChild;
                node.LeftChild.Parent = null;
            }

            node.LeftChild.SetAndLinkRightChild(node);
            node.SetAndLinkLeftChild(toLeftAndRightChild);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            var itr = GetEnumerator() as NotADictionaryEnumerator<K, T> ?? throw new Exception("ive messed up. like, really, REALLY bad");
            while(itr.MoveNext())
            {
                sb.Append($"{itr.CurrentNode}\n");
            }
            return sb.ToString();
        }
    }


    internal enum NodeColor
    {
        None,
        Black,
        Red
    }

    internal class RedBlackTreeNode<K,T> where K : IComparable<K>
    {
        public NodeColor color = NodeColor.None;
        public T value = default;
        public readonly K Key;
        public RedBlackTreeNode<K,T> Parent { get; internal set; }
        public RedBlackTreeNode<K,T> LeftChild { get; internal set; }
        public RedBlackTreeNode<K,T> RightChild { get; internal set; }

        public bool IsLeftChild => Parent?.LeftChild == this;                
        public bool IsRightChild => Parent?.RightChild == this;        
        
        internal void Unlink()
        {
            if (IsLeftChild)
                Parent.LeftChild = null;
            else if (IsRightChild)
                Parent.RightChild = null;

            if (LeftChild?.Parent == this)
                LeftChild.Parent = null;

            if (RightChild?.Parent == this)
                RightChild.Parent = null;

            // Parent = null;
            // LeftChild = null;
            // RightChild = null;
        }

        public void SetAndLinkLeftChild(RedBlackTreeNode<K,T> child)
        {
            LeftChild = child;
            if (child != null)
            {
                child.Parent = this;
            }
        }

        public void SetAndLinkRightChild(RedBlackTreeNode<K,T> child)
        {
            RightChild = child;
            if (child != null)
            {
                child.Parent = this;
            }
        }

        public RedBlackTreeNode(K key, T value = default)
        {
            Key = key;
            Parent = null;
            LeftChild = null;
            RightChild = null;
            this.value = value;
        }

        public override string ToString()
        {
            return $"[{Key}]: {value}";
        }
    }


    internal class NotADictionaryEnumerator<K, T> : IEnumerator<KeyValuePair<K, T>> where K : IComparable<K>
    {
        private RedBlackTreeNode<K, T> root;
        private RedBlackTreeNode<K, T> current = null;
        private RedBlackTreeNode<K, T> next;


        public NotADictionaryEnumerator(RedBlackTreeNode<K, T> root)
        {
            this.root = root;
            next = root;
        }

        public KeyValuePair<K, T> Current => new KeyValuePair<K, T>(current.Key, current.value);

        public RedBlackTreeNode<K, T> CurrentNode => current;

        object IEnumerator.Current => new KeyValuePair<K, T>(current.Key, current.value);

        public void Dispose()
        {
            root = null;
            current = null;
            next = null;
        }

        public bool MoveNext()
        {
            if (next == null)
                return false;

            current = next;

            var nextNode = next.LeftChild ?? next.RightChild;
            if (nextNode == null)
            {
                nextNode = next;
                while (nextNode!=null && (nextNode.IsRightChild || nextNode.Parent?.RightChild==null))
                    nextNode = nextNode.Parent;

                nextNode = nextNode?.Parent?.RightChild;
            }
            next = nextNode;

            return true;
        }

        public void Reset()
        {
            current = null;
            next = root;
        }
    }
}



