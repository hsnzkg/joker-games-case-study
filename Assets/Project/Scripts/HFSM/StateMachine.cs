using System;
using System.Collections.Generic;

namespace Project.Scripts.HFSM
{
    public sealed class StateMachine
    {
        private readonly Dictionary<Type, StateNode> m_nodes = new();
        private readonly HashSet<ITransition> m_anyTransitions = new();
        private StateNode m_defaultNode;
        private StateNode m_activeNode;
        private StateNode m_previousNode;
        private IRuntimeMode m_runtimeMode;
        private readonly HierarchicalStateDispatcher m_hierarchicalStateDispatcher;

        public bool Debug { get; }


        public StateMachine(IRuntimeMode mode, bool debug = false)
        {
            Debug = debug;
            m_hierarchicalStateDispatcher = new HierarchicalStateDispatcher();
            SetRuntimeMode(mode);
        }

        internal void SetRuntimeMode(IRuntimeMode mode)
        {
            m_runtimeMode = mode;
        }

        #region Node

        private void CreateNode(StateBase stateBase)
        {
            StateNode node = new(stateBase);
            Type stateType = stateBase.GetType();
            m_nodes.TryAdd(stateType, node);
        }

        private StateNode GetNode(StateBase state)
        {
            return m_nodes.GetValueOrDefault(state.GetType());
        }

        private StateNode GetNode<T>()
        {
            if (!m_nodes.TryGetValue(typeof(T), out StateNode node))
            {
                UnityEngine.Debug.Log($"[FSM] : The state of type {typeof(T).Name} does not exist");
            }

            return node;
        }

        #endregion

        #region Check & Validate

        public void Update()
        {
            if (m_activeNode == null && m_defaultNode == null)
            {
                UnityEngine.Debug.LogError("[FSM] : There is no active and default state !");
                return;
            }

            if (m_activeNode == null)
            {
                ChangeStateToDefaultState();
            }

            bool hasTransition = m_runtimeMode.Tick(this);
            if (!hasTransition) Dispatch(m_hierarchicalStateDispatcher);
        }

        #endregion

        #region State

        public void AddState(StateBase state)
        {
            if (HasState(state))
            {
                UnityEngine.Debug.LogWarning($"[FSM] {state.GetType().Name} already exists");
                return;
            }

            CreateNode(state);
        }

        public void AddState<T>(params object[] constructorParams) where T : StateBase
        {
            if (HasState<T>())
            {
                UnityEngine.Debug.LogWarning($"[FSM] State {typeof(T).Name} same type already exists");
                return;
            }

            T state = (T)Activator.CreateInstance(typeof(T), args: constructorParams);
            CreateNode(state);
        }

        public void SetDefaultState<T>()
        {
            m_defaultNode = GetNode<T>();
        }

        public void SetDefaultState(StateBase child)
        {
            m_defaultNode = GetNode(child);
        }

        internal void ChangeStateInternal(StateBase nextState)
        {
            if (!HasState(nextState))
            {
                UnityEngine.Debug.LogError($"[FSM] State {nextState.GetType().Name} not found");
                return;
            }

            if (m_activeNode != null && m_activeNode.StateBase == nextState)
            {
                return;
            }

            StateBase current = GetLeafState();
            StateBase previousState = current;

            while (current != null)
            {
                if (Debug)
                {
                    UnityEngine.Debug.Log($"[FSM] State Machine Exit: {current.GetType().Name}");
                }
                
                current.OnExitInternal();

                // Clear child FSM active node so re-entry starts fresh
                if (current.IsHierarchical)
                {
                    current.StateMachine.ClearActiveState();
                }

                current.Parent?.OnChildExitedInternal(current);

                if (current == m_activeNode?.StateBase) break;

                current = current.Parent;
            }

            m_previousNode = m_activeNode;
            m_activeNode = m_nodes[nextState.GetType()];
            
            if (Debug)
            {
                string fromState = previousState?.GetType().Name ?? "None";
                string toState = nextState.GetType().Name;
                UnityEngine.Debug.Log($"[FSM] State Machine Transition: [{fromState}] -> [{toState}]");
            }
            
            nextState.OnEnterInternal();
            nextState.Parent?.OnChildEnteredInternal(nextState);
        }

        public void ChangeState<T>()
        {
            if (HasState<T>())
            {
                ChangeStateInternal(GetState<T>());
                return;
            }

            if (HasDeepState<T>())
            {
                if (!TryChangeStateDeep<T>())
                {
                    UnityEngine.Debug.LogError($"[FSM] : Deep state {typeof(T).Name} was found but transition failed !");
                }

                return;
            }

            UnityEngine.Debug.LogError($"[FSM] : The state of type {typeof(T).Name} could not be found !");
        }

        private bool TryChangeStateDeep<T>()
        {
            if (HasState<T>())
            {
                ChangeStateInternal(GetState<T>());
                return true;
            }

            foreach (StateNode node in m_nodes.Values)
            {
                StateBase parentState = node.StateBase;
                StateMachine childFsm = parentState.StateMachine;

                if (childFsm == null)
                    continue;

                if (childFsm.HasState<T>() || childFsm.HasDeepState<T>())
                {
                    if (m_activeNode == null || m_activeNode.StateBase != parentState)
                    {
                        ChangeStateInternal(parentState);
                    }

                    return childFsm.TryChangeStateDeep<T>();
                }
            }

            return false;
        }

        private void ChangeStateToDefaultState()
        {
            if (m_defaultNode.StateBase == null)
            {
                UnityEngine.Debug.LogError($"[FSM] : Default state is not valid !");
            }
            else
            {
                // Check if there's a valid transition before going to default
                if (HasValidTransition(out ITransition transition))
                {
                    if (Debug)
                    {
                        UnityEngine.Debug.Log($"[FSM] Aborting default state, there is valid transition to {transition.To.GetType().Name}");
                    }
                    ChangeStateInternal(transition.To);
                }
                else
                {
                    if (Debug)
                    {
                        UnityEngine.Debug.Log($"[FSM] Entering Default State: {m_defaultNode.StateBase.GetType().Name}");
                    }
                    ChangeStateInternal(m_defaultNode.StateBase);
                }
            }
        }

        public StateBase GetState<T>()
        {
            return GetNode<T>().StateBase;
        }

        public StateBase GetActiveState()
        {
            return m_activeNode?.StateBase;
        }

        /// <summary>
        /// Clears the active state node, recursively clearing any child FSMs.
        /// This ensures that when a hierarchical state is re-entered, 
        /// the child FSM will properly go through default state initialization.
        /// </summary>
        internal void ClearActiveState()
        {
            if (m_activeNode == null) return;
            
            // Recursively clear nested child FSMs
            if (m_activeNode.StateBase.IsHierarchical)
            {
                m_activeNode.StateBase.StateMachine.ClearActiveState();
            }
            
            m_activeNode = null;
        }

        public StateBase GetLeafState()
        {
            if (m_activeNode == null) return null;
            StateBase current = m_activeNode.StateBase;

            while (current.IsHierarchical)
            {
                StateMachine childFsm = current.StateMachine;
                StateBase childActive = childFsm?.GetActiveState();
                if (childActive == null) break;
                current = childActive;
            }

            return current;
        }

        public StateBase GetRootState()
        {
            if (m_activeNode == null) return null;

            StateBase current = m_activeNode.StateBase;

            while (current.Parent != null)
            {
                current = current.Parent;
            }

            return current;
        }


        internal bool HasState(Type stateType)
        {
            return m_nodes.ContainsKey(stateType);
        }

        internal bool HasState(StateBase state)
        {
            return HasState(state.GetType());
        }

        internal bool HasState<T>()
        {
            return HasState(typeof(T));
        }

        internal bool HasDeepState<T>()
        {
            foreach (StateNode node in m_nodes.Values)
            {
                StateMachine childFsm = node.StateBase.StateMachine;
                if (childFsm == null) continue;

                if (childFsm.HasState<T>() || childFsm.HasDeepState<T>())
                    return true;
            }

            return false;
        }

        #endregion

        #region Transition

        public void AddTransition(StateBase from, StateBase to, IPredicate condition)
        {
            bool fromValid = HasState(from);
            bool toValid = HasState(to);
            if (!fromValid || !toValid)
            {
                UnityEngine.Debug.LogError(
                    $"[FSM] : The state of type {(!fromValid ? from.GetType().Name : to.GetType().Name)} does not exist !");
                return;
            }

            GetNode(from).AddTransition(to, condition);
        }

        public void AddTransition<TFrom, TTo>(IPredicate condition) where TFrom : StateBase where TTo : StateBase
        {
            bool fromValid = HasState<TFrom>();
            bool toValid = HasState<TTo>();
            if (!fromValid || !toValid)
            {
                UnityEngine.Debug.LogError(
                    $"[FSM] : The state of type {(!fromValid ? typeof(TFrom).Name : typeof(TTo).Name)} does not exist !");
                return;
            }

            GetNode(GetState<TFrom>()).AddTransition(GetState<TTo>(), condition);
        }

        public void AddAnyTransition<TTo>(IPredicate condition)
        {
            if (!HasState<TTo>())
            {
                UnityEngine.Debug.LogError($"[FSM] : The state of type {typeof(TTo).Name} does not exist !");
                return;
            }

            m_anyTransitions.Add(new Transition(GetNode<TTo>().StateBase, condition));
        }

        public void AddAnyTransition(StateBase to, IPredicate condition)
        {
            if (!HasState(to))
            {
                UnityEngine.Debug.LogError($"[FSM] : The state of type {to.GetType().Name} does not exist !");
                return;
            }

            m_anyTransitions.Add(new Transition(GetNode(to).StateBase, condition));
        }

        internal bool HasValidTransition(out ITransition transition)
        {
            transition = null;
            foreach (ITransition t in m_anyTransitions)
            {
                if (m_activeNode != null && m_activeNode.StateBase == t.To) continue;
                if (!t.Condition.Evaluate()) continue;
                transition = t;
                return true;
            }
            
            StateNode nodeToCheck = m_activeNode ?? m_defaultNode;
            if (nodeToCheck != null)
            {
                foreach (ITransition t in nodeToCheck.Transitions)
                {
                    if (!t.Condition.Evaluate()) continue;
                    transition = t;
                    return true;
                }
            }
            
            return false;
        }

        #endregion

        #region Hierarchical Dispatch

        #region Dispatch

        public void Dispatch(IDispatcher visitor)
        {
            if (m_activeNode == null) return;
            DispatchRecursive(m_activeNode.StateBase, visitor);
        }

        private void DispatchRecursive(StateBase state, IDispatcher visitor)
        {
            if (state == null) return;
            state.Handle(visitor);
            if (!state.IsHierarchical) return;
            StateBase child = state.StateMachine.GetActiveState();
            DispatchRecursive(child, visitor);
        }

        #endregion


        #region Dispatch Ref

        public void Dispatch<TContext>(IRefDispatcher<TContext> dispatcher, ref TContext context)
        {
            if (m_activeNode == null) return;
            DispatchRecursive(m_activeNode.StateBase, dispatcher, ref context);
        }

        private void DispatchRecursive<TContext>(StateBase state, IRefDispatcher<TContext> dispatcher, ref TContext context)
        {
            if (state == null) return;
            dispatcher.Dispatch(state, ref context);

            if (!state.IsHierarchical) return;

            StateBase child = state.StateMachine.GetActiveState();
            DispatchRecursive(child, dispatcher, ref context);
        }

        #endregion


        #region Dispatch Value

        public void Dispatch<TContext>(IValueDispatcher<TContext> dispatcher, TContext context)
        {
            if (m_activeNode == null) return;
            DispatchRecursive(m_activeNode.StateBase, dispatcher, context);
        }

        private void DispatchRecursive<TContext>(StateBase state, IValueDispatcher<TContext> dispatcher, TContext context)
        {
            if (state == null) return;
            dispatcher.Dispatch(state, context);

            if (!state.IsHierarchical) return;

            StateBase child = state.StateMachine.GetActiveState();
            DispatchRecursive(child, dispatcher, context);
        }

        #endregion
        
        #endregion
    }
}
