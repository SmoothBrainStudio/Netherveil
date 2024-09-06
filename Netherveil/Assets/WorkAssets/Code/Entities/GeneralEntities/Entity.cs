using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.VFX;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class Entity : MonoBehaviour
{
    public static int entitySpawn = 0;
    public Action OnFreeze;
    public enum EntityState : int
    {
        MOVE,
        ATTACK,
        HIT,
        DEAD,
        NB
    }
    // Only to save EditorChange
    [SerializeField] List<string> statusNameToApply = new List<string>();
    [SerializeField] List<float> durationStatusToApply = new List<float>();
    [SerializeField] List<float> chanceStatusToApply = new List<float>();

    [Header("Properties")]
    [SerializeField] protected Stats stats;

    public delegate void DeathDelegate(Vector3 vector);
    public DeathDelegate OnDeath;
    public event Action OnChangeState;

    public List<Status> AppliedStatusList = new();
    protected List<Status> statusToApply = new();
    public List<VisualEffect> statusVfxs = new();
    public bool IsKnockbackable = true;
    public bool canTriggerTraps = true;
    public byte isFreeze = 0;
    public bool IsFreeze 
    { 
        get 
        { 
            return isFreeze > 0; 
        } 
        protected set
        {
            switch(value)
            {
                case true:
                    if(IsFreeze)
                    {
                        isFreeze = 1;
                    }
                    break;
                case false:
                    isFreeze = 0;
                    break;
            }
        }
    }

    public class InvincibilityCount
    {
        /// <summary>
        /// dont increment/decrement this value, use the class to increment/decrement
        /// </summary>
        public int Value { get; set; } = 0;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static InvincibilityCount operator +(InvincibilityCount invincibilityCount, int increment)
        {
            invincibilityCount.Value += increment;
            invincibilityCount.Value = Mathf.Max(0, invincibilityCount.Value);
            return invincibilityCount;
        }

        public static InvincibilityCount operator -(InvincibilityCount invincibilityCount, int decrement)
        {
            invincibilityCount.Value -= decrement;
            invincibilityCount.Value = Mathf.Max(0, invincibilityCount.Value);
            return invincibilityCount;
        }

        public static InvincibilityCount operator ++(InvincibilityCount invincibilityCount)
        {
            invincibilityCount.Value++;
            invincibilityCount.Value = Mathf.Max(0, invincibilityCount.Value);
            return invincibilityCount;
        }

        public static InvincibilityCount operator --(InvincibilityCount invincibilityCount)
        {
            invincibilityCount.Value--;
            invincibilityCount.Value = Mathf.Max(0, invincibilityCount.Value);
            return invincibilityCount;
        }

        public static bool operator ==(InvincibilityCount invincibilityCount, int toCompare)
        {
            return invincibilityCount.Value == toCompare;
        }

        public static bool operator !=(InvincibilityCount invincibilityCount, int toCompare)
        {
            return invincibilityCount.Value != toCompare;
        }

        public static bool operator >(InvincibilityCount invincibilityCount, int toCompare)
        {
            return invincibilityCount.Value > toCompare;
        }

        public static bool operator <(InvincibilityCount invincibilityCount, int toCompare)
        {
            return invincibilityCount.Value < toCompare;
        }

        public static bool operator <=(InvincibilityCount invincibilityCount, int toCompare)
        {
            return invincibilityCount.Value <= toCompare;
        }

        public static bool operator >=(InvincibilityCount invincibilityCount, int toCompare)
        {
            return invincibilityCount.Value >= toCompare;
        }
    }

    //not a bool because if you get multiple invincibility sources at the same time,
    //if one would go away, he would put the bool at false but it would break the invincibility from other sources too,
    //so it is a count that means the entity is invincible if this variable is over zero.
    public InvincibilityCount IsInvincibleCount = new();

    private int state = (int)EntityState.MOVE;
    public int State
    {
        get { return state; }
        set
        {
            state = value;
            OnChangeState?.Invoke();
        }
    }

    public Stats Stats
    {
        get
        {
            return stats;
        }
    }

    protected virtual void Awake()
    {
        for (int i = 0; i < statusNameToApply.Count; i++)
        {
            Type statusType = Assembly.GetExecutingAssembly().GetType(statusNameToApply[i]);
            ConstructorInfo constructor = statusType.GetConstructor(new[] { typeof(float), typeof(float) });
            if (constructor != null)
            {
                statusToApply.Add((Status)constructor.Invoke(new object[] { durationStatusToApply[i], chanceStatusToApply[i] }));
            }           

        }
    }
    protected virtual void Start()
    {
        OnDeath += ctx => ClearStatus();

        entitySpawn++;
        if (this is IAttacker attacker)
        {
            attacker.OnAttackHit += attacker.ApplyStatus;
        }
    }

    protected virtual void Update()
    {
        if (AppliedStatusList.Count > 0)
        {
            for (int i = AppliedStatusList.Count - 1; i >= 0; i--)
            {
                if (AppliedStatusList[i].isFinished)
                {
                    AppliedStatusList.RemoveAt(i);
                }
            }
        }
    }

    protected virtual void OnTriggerStay(Collider collider)
    {
        //if (collider.gameObject.TryGetComponent(out Entity other))
        //{
        //    CheckKnockbackCollision(other);
        //}
    }

    protected virtual void CheckKnockbackCollision(Entity other)
    {
        if (!this.IsKnockbackable || !other.IsKnockbackable) // one entity can't be knockback : don't need to check anything
        {
            return;
        }

        Knockback otherKnockback = other.GetComponent<Knockback>();
        Knockback thisKnockback = this.GetComponent<Knockback>();

        if (otherKnockback.IsKnockback && !thisKnockback.IsKnockback)
        {
            this.ApplyKnockback(this.GetComponent<IDamageable>(), other.GetComponent<IAttacker>(), Vector3.Distance(otherKnockback.transform.position, otherKnockback.endKnockback));
        }
    }

    public void ApplyKnockback(IDamageable damageable, IAttacker attacker)
    {
        Vector3 temp = (damageable as MonoBehaviour).transform.position - transform.position;
        Vector3 direction = new Vector3(temp.x, 0f, temp.z).normalized;

        float distance = stats.GetValue(Stat.KNOCKBACK_DISTANCE);
        float speed = stats.GetValue(Stat.KNOCKBACK_COEFF);

        ApplyKnockback(damageable, attacker, direction, distance, speed);
    }

    public void ApplyKnockback(IDamageable damageable, IAttacker attacker, float distance = -1f, float speed = -1f)
    {
        Vector3 temp = (damageable as MonoBehaviour).transform.position - transform.position;
        Vector3 direction = new Vector3(temp.x, 0f, temp.z).normalized;

        if (distance < 0f)
        {
            distance = stats.GetValue(Stat.KNOCKBACK_DISTANCE);
        }
        if (speed < 0f)
        {
            speed = stats.GetValue(Stat.KNOCKBACK_COEFF);
        }

        ApplyKnockback(damageable, attacker, direction, distance, speed);
    }

    public void ApplyKnockback(IDamageable damageable, IAttacker attacker, Vector3 direction)
    {
        float distance = stats.GetValue(Stat.KNOCKBACK_DISTANCE);
        float speed = stats.GetValue(Stat.KNOCKBACK_COEFF);

        ApplyKnockback(damageable, attacker, direction, distance, speed);
    }

    public void ApplyKnockback(IDamageable damageable, IAttacker attacker, Vector3 direction, float distance, float speed)
    {
        if (distance > 0f)
        {
            Knockback knockbackable = (damageable as MonoBehaviour).GetComponent<Knockback>();
            if (knockbackable && (damageable as MonoBehaviour).GetComponent<Entity>().IsKnockbackable)
            {
                MonoBehaviour damageableGO = damageable as MonoBehaviour;
                Vector3 damageablePos = damageableGO.transform.position;
                Vector3 TargetToMeVec = (transform.position - damageablePos).normalized;

                if (TargetToMeVec != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(new Vector3(TargetToMeVec.x, 0f, TargetToMeVec.z));
                    rotation *= Camera.main.transform.rotation;
                    float rotationY = rotation.eulerAngles.y;

                    if (damageableGO.TryGetComponent(out PlayerController controller) && damageableGO.GetComponent<Entity>().state != (int)EntityState.DEAD)
                    {
                        controller.OverridePlayerRotation(rotationY, true);
                    }
                }

                knockbackable.GetKnockback(attacker, direction, distance, speed);
                FloatingTextGenerator.CreateActionText(damageableGO.transform.position, "Pushed!");
            }
        }
    }

    /// <summary>
    /// Try to add a status and return false if it didn't work
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public bool AddStatus(Status status, IAttacker attacker = null)
    {
        if (status == null)
        {
            Debug.LogError("Can't add status on " + this.name + " because status is null");
            return false;
        }
        if (!status.CanApplyEffect(this))
        {
            Debug.Log("Status can't be applied on " + this.name);
            return false;
        }
        if (status.GetType() == typeof(Electricity) || status.GetType() == typeof(Freeze)) return false;
        else
        {
            if(this.IsInvincibleCount > 0 || (this is Mobs && (this as Mobs).IsSpawning))
                return false;

            float chance = UnityEngine.Random.value;
            if (chance <= status.statusChance)
            {
                // If status already exist add a stack
                foreach (var appliedStatus in AppliedStatusList)
                {
                    if (appliedStatus.GetType() == status.GetType())
                    {
                        appliedStatus.AddStack(1);
                        return true;
                    }
                }
                // Else add the status
                status.target = this;
                status.launcher = attacker;
                status.ApplyEffect(this);
                this.AppliedStatusList.Add(status);
                return true;
            }
            return false;
        }
    }

    protected void ClearStatus()
    {
        foreach (var status in AppliedStatusList)
        {
            status.isFinished = true;
        }
        foreach (var vfx in statusVfxs)
        {
            vfx.GetComponent<VFXStopper>().StopAllCoroutines();
            vfx.Stop();
            Destroy(vfx.gameObject);
        }
        AppliedStatusList.Clear();
        statusVfxs.Clear();
    }

    
}

#if UNITY_EDITOR
[CustomEditor(typeof(Entity), true), CanEditMultipleObjects]
public class EntityDrawer : Editor
{
    SerializedProperty statProperty;

    // Status name list in the entity ( required to instantiate with the reflection )
    SerializedProperty statusNameListProperty;

    // Duration of each status in the entity ( required to instantiate with the reflection )
    SerializedProperty statusDurationListProperty;

    // Duration of each status in the entity ( required to instantiate with the reflection )
    SerializedProperty statusChanceListProperty;

    // Index of each status ( in the name list )
    List<int> allIndex = new();
    bool isStatusExpended = false;
    int nbStatus = 0;

    // Local lists that we will use to update properties
    List<string> statusNameList = new();
    List<float> durationList = new();
    List<float> chanceList = new();

    List<string> classField = new();
    List<bool> foldoutList = new();
    private void OnEnable()
    {
        statProperty = serializedObject.FindProperty("stats");
        statusNameListProperty = serializedObject.FindProperty("statusNameToApply");
        statusDurationListProperty = serializedObject.FindProperty("durationStatusToApply");
        statusChanceListProperty = serializedObject.FindProperty("chanceStatusToApply");

        // Add existing Status name in a list
        if (statusNameList.Count == 0)
        {
            var typeList = Assembly.GetExecutingAssembly().GetTypes().Where(type =>
            {
                return type.IsSubclassOf(typeof(Status)) && !type.IsAbstract;
            });

            foreach (Type status in typeList)
            {
                statusNameList.Add(status.Name);
            }
        }

        // Update local lists with the entity lists
        for (int i = 0; i < statusNameListProperty.arraySize; i++)
        {
            nbStatus++;
            int indexOfString = statusNameList.IndexOf(statusNameListProperty.GetArrayElementAtIndex(i).stringValue);
            if (indexOfString != -1)
            {
                allIndex.Add(indexOfString);
            }

            durationList.Add(statusDurationListProperty.GetArrayElementAtIndex(i).floatValue);
            chanceList.Add(statusChanceListProperty.GetArrayElementAtIndex(i).floatValue);
        }

        // Get all infos
        List<FieldInfo> infos = new();
        Type currentType = target.GetType();
        List<FieldInfo[]> fieldInfos = new();
        while (currentType != typeof(Entity) && currentType != typeof(object) && currentType != null)
        {
            fieldInfos.Add(currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            // If doesn't inherit, return type System.Object ( that's equal to object )
            currentType = currentType.BaseType;
        }
        fieldInfos.Reverse();

        for (int j = 0; j < fieldInfos.Count; j++)
        {
            foreach (var coucou in fieldInfos[j])
            {
                infos.Add(coucou);
            }
        }
        for (int i = 0; i < infos.Count; i++)
        {
            if ((infos[i].IsPublic && !infos[i].IsInitOnly && infos[i].GetCustomAttribute(typeof(HideInInspector)) == null) || infos[i].GetCustomAttribute(typeof(SerializeField)) != null)
            {
                classField.Add(infos[i].Name);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawScript();
        EditorGUILayout.PropertyField(statProperty);

        EditorGUILayout.BeginHorizontal();
        isStatusExpended = EditorGUILayout.Foldout(isStatusExpended, "Status");
        EditorGUILayout.EndHorizontal();

        if (isStatusExpended)
        {
            for (int i = 0; i < nbStatus; i++)
            {
                AddValueInEachList(i);
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                foldoutList[i] = EditorGUILayout.Foldout(foldoutList[i], statusNameList.ToArray()[allIndex[i]]);
                GUI.color = Color.red;
                if (GUILayout.Button("X", GUILayout.MaxWidth(75)))
                {
                    allIndex.RemoveAt(i);
                    durationList.RemoveAt(i);
                    chanceList.RemoveAt(i);
                    statusNameListProperty.DeleteArrayElementAtIndex(i);
                    statusDurationListProperty.DeleteArrayElementAtIndex(i);
                    statusChanceListProperty.DeleteArrayElementAtIndex(i);
                    nbStatus--;
                    serializedObject.ApplyModifiedProperties();
                    return;
                }
                GUI.color = Color.white;

                EditorGUILayout.EndHorizontal();

                if (foldoutList[i])
                {
                    // popup to choose the index of the status in the name List
                    allIndex[i] = EditorGUILayout.Popup("Status", allIndex[i], statusNameList.ToArray());
                    durationList[i] = EditorGUILayout.FloatField("Duration", durationList[i]);
                    chanceList[i] = EditorGUILayout.Slider("Chance", chanceList[i], 0, 1);

                    // Update properties

                }
                statusNameListProperty.GetArrayElementAtIndex(i).stringValue = statusNameList[allIndex[i]];
                statusDurationListProperty.GetArrayElementAtIndex(i).doubleValue = durationList[i];
                statusChanceListProperty.GetArrayElementAtIndex(i).doubleValue = chanceList[i];
                EditorGUI.indentLevel--;

            }

            // Button to add a new status in the list
            GUI.color = Color.green;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                statusNameListProperty.InsertArrayElementAtIndex(nbStatus);
                statusDurationListProperty.InsertArrayElementAtIndex(nbStatus);
                statusChanceListProperty.InsertArrayElementAtIndex(nbStatus);
                allIndex.Add(0);
                durationList.Add(0);
                chanceList.Add(0);
                nbStatus++;
            }
            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;
        }

        foreach (string fieldToDisplay in classField)
        {
            try
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(fieldToDisplay));
            }
            catch (Exception)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("No GUI implemented for : " + fieldToDisplay);
                EditorGUILayout.EndHorizontal();
            }

        }

        serializedObject.ApplyModifiedProperties();

    }

    private void AddValueInEachList(int i)
    {
        if (foldoutList.Count <= i)
        {
            foldoutList.Add(false);
        }
        if (allIndex.Count <= i)
        {
            allIndex.Add(0);
        }
        if (durationList.Count <= i)
        {
            durationList.Add(0);
        }
        if (chanceList.Count <= i)
        {
            chanceList.Add(0);
        }
        if (statusDurationListProperty.arraySize <= i)
        {
            statusDurationListProperty.InsertArrayElementAtIndex(i);
        }
        if (statusChanceListProperty.arraySize <= i)
        {
            statusChanceListProperty.InsertArrayElementAtIndex(i);
        }
    }

    void DrawScript()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(true);
        MonoScript script = MonoScript.FromMonoBehaviour((Entity)target);
        EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
    }
    public static bool HasAttribute(Type t)
    {
        // Get instance of the attribute.
        SerializeField MyAttribute =
            (SerializeField)Attribute.GetCustomAttribute(t, typeof(SerializeField));

        if (MyAttribute == null)
        {
            Console.WriteLine("The attribute has not be found.");
            return false;
        }
        else
        {
            return true;
        }
    }
}
#endif
