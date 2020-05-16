using System.Collections;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class RedBlackTree<TValue> : IEnumerable<TValue>
    {
        #region Nested types

        public enum NodeColor
        {
            Red,
            Black
        }

        public sealed class Node
        {
            #region Properties

            public TValue Value { get; set; }

            public Node Left { get; set; }

            public Node Right { get; set; }

            public Node Parent { get; set; }

            public NodeColor Color { get; set; }

            #endregion
        }

        public sealed class Enumerator : IEnumerator<TValue>
        {
            #region Fields

            private readonly Node _root;
            private Node _current;

            #endregion

            #region Constructor

            public Enumerator(Node root)
            {
                _root = root;
            }

            #endregion

            #region Properties

            public TValue Current => _current.Value;

            object IEnumerator.Current => Current;

            #endregion

            #region Methods

            public bool MoveNext()
            {
                /*
                 if (root != null)
                 {
                      InOrder_Rec(root.Left);
                      Console.Write(root.Data +" ");
                      InOrder_Rec(root.Right);
                }
                 */


                return false;
            }

            public void Reset()
            {
                _current = _root;
            }

            public void Dispose()
            {
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly IComparer<TValue> _comparer;

        private Node _root;

        #endregion

        #region Constructor

        public RedBlackTree(IComparer<TValue> comparer)
        {
            _comparer = comparer;
        }

        #endregion

        #region Methods

        public void Add(TValue value)
        {
            var node = new Node { Value = value };
            _root = Insert(null, node);
        }

        private Node GetParent(Node node)
        {
            return node?.Parent;
        }

        private Node GetGrandParent(Node node)
        {
            return GetParent(GetParent(node));
        }

        private Node GetSibling(Node node)
        {
            var parent = GetParent(node);
            if (parent == null)
                return null;

            return node == parent.Left ? parent.Right : parent.Left;
        }

        private Node GetUncle(Node node)
        {
            var parent = GetParent(node);
            return GetSibling(parent);
        }

        private void RotateLeft(Node node)
        {
            var newNode = node.Right;

            node.Right = newNode.Left;
            newNode.Left = node;
            node.Parent = newNode;
            
            if (node.Right != null)
                node.Right.Parent = node;

            var parent = GetParent(node);
            if (parent != null)
            {
                if (node == parent.Left)
                    parent.Left = newNode;
                else if (node == parent.Right)
                    parent.Right = newNode;
            }

            newNode.Parent = parent;
        }

        private void RotateRight(Node node)
        {
            var newNode = node.Left;

            node.Left = newNode.Right;
            newNode.Right = node;
            node.Parent = newNode;

            if (node.Left != null)
                node.Left.Parent = node;

            var parent = GetParent(node);
            if (parent != null)
            {
                if (node == parent.Left)
                    parent.Left = newNode;
                else if (node == parent.Right)
                    parent.Right = newNode;
            }

            newNode.Parent = parent;
        }

        private Node Insert(Node root, Node node)
        {
            InsertRecurse(root, node);
            InsertRepairTree(node);

            root = node;
            while (GetParent(root) != null)
            {
                root = GetParent(root);
            }

            return root;
        }

        private void InsertRecurse(Node root, Node node)
        {
            if (root != null)
            {
                if (_comparer.Compare(node.Value, root.Value) < 0)
                {
                    if (root.Left != null)
                    {
                        InsertRecurse(root.Left, node);
                        return;
                    }
                    else
                        root.Left = node;
                }
                else
                {
                    if (root.Right != null)
                    {
                        InsertRecurse(root.Right, node);
                        return;
                    }
                    else
                        root.Right = node;
                }
            }

            node.Parent = root;
            node.Left = null;
            node.Right = null;
            node.Color = NodeColor.Red;
        }

        private void InsertRepairTree(Node node)
        {
            if (GetParent(node) == null)
                InsertCase1(node);
            else if (GetParent(node).Color == NodeColor.Black)
                InsertCase2(node);
            else if (GetUncle(node) != null && GetUncle(node).Color == NodeColor.Red)
                InsertCase3(node);
            else
                InsertCase4(node);
        }

        private void InsertCase1(Node node)
        {
            node.Color = NodeColor.Black;
        }

        private void InsertCase2(Node node)
        {
            return;
        }

        private void InsertCase3(Node node)
        {
            GetParent(node).Color = NodeColor.Black;
            GetUncle(node).Color = NodeColor.Black;
            GetGrandParent(node).Color = NodeColor.Red;
            InsertRepairTree(GetGrandParent(node));
        }

        private void InsertCase4(Node node)
        {
            var parent = GetParent(node);
            var grandParent = GetGrandParent(node);

            if (node == parent.Right && parent == grandParent.Left)
            {
                RotateLeft(parent);
                node = node.Left;
            }
            else if (node == parent.Left && parent == grandParent.Right)
            {
                RotateRight(parent);
                node = node.Right;
            }

            InsertCase4Step2(node);
        }

        private void InsertCase4Step2(Node node)
        {
            var parent = GetParent(node);
            var grandParent = GetGrandParent(node);

            if (node == parent.Left)
                RotateRight(grandParent);
            else
                RotateLeft(grandParent);

            parent.Color = NodeColor.Black;
            grandParent.Color = NodeColor.Red;
        }

        private void ReplaceNode(Node node, Node child)
        {
            child.Parent = node.Parent;
            if (node == node.Parent.Left)
                node.Parent.Left = child;
            else
                node.Parent.Right = child;
        }

        private void DeleteOneChild(Node node)
        {
            var child = node.Right ?? node.Left;

            ReplaceNode(node, child);
            if (node.Color == NodeColor.Black)
            {
                if (child.Color == NodeColor.Red)
                    child.Color = NodeColor.Black;
                else
                    DeleteCase1(child);
            }
        }

        private void DeleteCase1(Node node)
        {
            if (node.Parent != null)
                DeleteCase2(node);
        }

        private void DeleteCase2(Node node)
        {
            var sibling = GetSibling(node);
            if (sibling.Color == NodeColor.Red)
            {
                node.Parent.Color = NodeColor.Red;
                sibling.Color = NodeColor.Black;
                if (node == node.Parent.Left)
                    RotateLeft(node.Parent);
                else
                    RotateRight(node.Parent);
            }

            DeleteCase3(node);
        }

        private void DeleteCase3(Node node)
        {
            var sibling = GetSibling(node);

            if ((node.Parent.Color == NodeColor.Black) && (sibling.Color == NodeColor.Black) &&
                (sibling.Left.Color == NodeColor.Black) && (sibling.Right.Color == NodeColor.Black))
            {
                sibling.Color = NodeColor.Red;
                DeleteCase1(node.Parent);
            }
            else
                DeleteCase4(node);
        }

        private void DeleteCase4(Node node)
        {
            var sibling = GetSibling(node);

            if ((node.Parent.Color == NodeColor.Red) && (sibling.Color == NodeColor.Black) &&
                (sibling.Left.Color == NodeColor.Black) && (sibling.Right.Color == NodeColor.Black))
            {
                sibling.Color = NodeColor.Red;
                node.Parent.Color = NodeColor.Black;
            }
            else
                DeleteCase5(node);
        }

        private void DeleteCase5(Node node)
        {
            var sibling = GetSibling(node);

            if (sibling.Color == NodeColor.Black)
            {
                if ((node == node.Parent.Left) && (sibling.Right.Color == NodeColor.Black) &&
                    (sibling.Left.Color == NodeColor.Red))
                {
                    sibling.Color = NodeColor.Red;
                    sibling.Left.Color = NodeColor.Black;
                    RotateRight(sibling);
                }
                else if ((node == node.Parent.Right) && (sibling.Left.Color == NodeColor.Black) &&
                         (sibling.Right.Color == NodeColor.Red))
                {
                    sibling.Color = NodeColor.Red;
                    sibling.Right.Color = NodeColor.Black;
                    RotateLeft(sibling);
                }
            }

            DeleteCase6(node);
        }

        private void DeleteCase6(Node node)
        {
            var sibling = GetSibling(node);

            sibling.Color = node.Parent.Color;
            node.Parent.Color = NodeColor.Black;

            if (node == node.Parent.Left)
            {
                sibling.Right.Color = NodeColor.Black;
                RotateLeft(node.Parent);
            }
            else
            {
                sibling.Left.Color = NodeColor.Black;
                RotateRight(node.Parent);
            }
        }

        #endregion

        #region IEnumerable<TValue>

        public IEnumerator<TValue> GetEnumerator()
        {
            return new Enumerator(_root);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
