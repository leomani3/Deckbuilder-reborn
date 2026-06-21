using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Deckbuilder.Cards;
using Deckbuilder.Cards.Actions;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;
using Stats;

[CreateAssetMenu(menuName = "Config/GameAssets")]
public class GameAssets : ScriptableObject
{
    private static GameAssets _instance;
    public static GameAssets Instance => _instance ?? Load();

    private static GameAssets Load()
    {
        _instance = Resources.Load<GameAssets>("GameAssets");
        return _instance;
    }


    // ----------------------------------------------------------

    [AssetList(Path = "_GameAssets/Objects/Currencies/", AutoPopulate = true)]
    public CurrencyAsset[] currencyAssets;

    [AssetList(Path = "_GameAssets/Objects/CardActionDefinitions/", AutoPopulate = true)]
    public CardActionDefinition[] cardActionDefinitions;

    // [AssetList(Path = "_GameAssets/Stats/")]
    // public StatData[] statData;

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateCardActionDefinitions();
    }

    [Button("Validate Card Action Definitions")]
    private void ValidateCardActionDefinitions()
    {
        if (cardActionDefinitions == null)
            return;

        Dictionary<Type, List<CardActionDefinition>> _presentByType = new Dictionary<Type, List<CardActionDefinition>>();
        foreach (CardActionDefinition _definition in cardActionDefinitions)
        {
            if (_definition == null)
                continue;

            Type _type = _definition.GetType();
            if (!_presentByType.TryGetValue(_type, out List<CardActionDefinition> _list))
                _presentByType[_type] = _list = new List<CardActionDefinition>();

            _list.Add(_definition);
        }

        foreach (KeyValuePair<Type, List<CardActionDefinition>> _entry in _presentByType)
        {
            if (_entry.Value.Count > 1)
                Debug.LogError($"[GameAssets] Duplicate CardActionDefinition of type {_entry.Key.Name}: {string.Join(", ", _entry.Value.Select(_d => _d.name))}. Only one of these will be used, silently.", this);
        }

        foreach (Type _requiredType in GetRequiredDefinitionTypes())
        {
            if (!_presentByType.ContainsKey(_requiredType))
                Debug.LogError($"[GameAssets] Missing CardActionDefinition of type {_requiredType.Name}. Any CardAction of the matching kind will silently evaluate to Weight = 0.", this);
        }
    }

    private static IEnumerable<Type> GetRequiredDefinitionTypes()
    {
        IEnumerable<Type> _actionTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(GetLoadableTypes)
            .Where(_type => typeof(CardAction).IsAssignableFrom(_type) && !_type.IsAbstract);

        HashSet<Type> _requiredTypes = new HashSet<Type>();
        foreach (Type _actionType in _actionTypes)
        {
            if (Activator.CreateInstance(_actionType) is CardAction _action)
                _requiredTypes.Add(_action.DefinitionType);
        }

        return _requiredTypes;
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly _assembly)
    {
        try
        {
            return _assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException _e)
        {
            return _e.Types.Where(_type => _type != null);
        }
    }
#endif
}
