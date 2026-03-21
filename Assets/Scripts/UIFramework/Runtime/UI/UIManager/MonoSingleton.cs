using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// MonoBehavior单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T m_Instance = null;

        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
                }

                return m_Instance;
            }
        }

        public virtual void Dispose()
        {
            DestroyImmediate(this);
        }

        void OnDestroy()
        {
            m_Instance = null;
        }
    }
}