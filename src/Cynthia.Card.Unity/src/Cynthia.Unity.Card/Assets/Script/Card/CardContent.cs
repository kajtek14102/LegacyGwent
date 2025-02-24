﻿using Alsein.Extensions;
using Autofac;
using Cynthia.Card;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Localization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardContent : MonoBehaviour
{
    public Text CardNameText;
    public TextMeshProUGUI CardNameTextMesh;
    public Text TagsText;
    public TextMeshProUGUI TagsTextMesh;
    public Text CardInfoText;
    public TextMeshProUGUI CardInfoTextMesh;


    public Image Head;
    public Image Bottom;

    public Sprite NorthernRealmsHead;//北方
    public Sprite ScoiaTaelHead;//松鼠党
    public Sprite MonstersHead;//怪物
    public Sprite SkelligeHead;//群岛
    public Sprite NilfgaardHead;//帝国
    public Sprite NeutralHead;//中立

    public Sprite NorthernRealmsContent;//北方
    public Sprite ScoiaTaelContent;//松鼠党
    public Sprite MonstersContent;//怪物
    public Sprite SkelligeContent;//群岛
    public Sprite NilfgaardContent;//帝国
    public Sprite NeutralContent;//中立
    //--------

    public IDictionary<Faction, Sprite> HeadMap;
    public IDictionary<Faction, Sprite> ContentMap;

    public RectTransform Content;

    // Start is called before the first frame update
    void Init()
    {
        HeadMap = new Dictionary<Faction, Sprite>();
        ContentMap = new Dictionary<Faction, Sprite>();
        HeadMap[Faction.Monsters] = MonstersHead;
        HeadMap[Faction.Neutral] = NeutralHead;
        HeadMap[Faction.Nilfgaard] = NilfgaardHead;
        HeadMap[Faction.NorthernRealms] = NorthernRealmsHead;
        HeadMap[Faction.ScoiaTael] = ScoiaTaelHead;
        HeadMap[Faction.Skellige] = SkelligeHead;
        //---
        ContentMap[Faction.Monsters] = MonstersContent;
        ContentMap[Faction.Neutral] = NeutralContent;
        ContentMap[Faction.Nilfgaard] = NilfgaardContent;
        ContentMap[Faction.NorthernRealms] = NorthernRealmsContent;
        ContentMap[Faction.ScoiaTael] = ScoiaTaelContent;
        ContentMap[Faction.Skellige] = SkelligeContent;
    }


    public void SetCard(CardStatus cardStatus)
    {
        if (cardStatus.IsCardBack || cardStatus.Conceal)
        {
            return;
        }
        if (HeadMap == null || ContentMap == null)
        {
            Init();
        }
        Head.sprite = HeadMap[cardStatus.Faction];
        Bottom.sprite = ContentMap[cardStatus.Faction];

        var translator = DependencyResolver.Container.Resolve<LocalizationService>();

        CardInfoTextMesh.text = translator.IsContainsKey(cardStatus.Info) ? translator.GetText(cardStatus.Info) : translator.GetCardInfo(cardStatus.CardId);

        CardNameTextMesh.text = translator.GetCardName(cardStatus.CardId);

        TagsTextMesh.text = cardStatus.Categories.Select(x => GwentMap.CategorieInfoMap[x])
            .ForAll(t => t = translator.GetText($"CardTag_{t}")).Join(", ");

        var immuneTag = translator.GetText("CardTag_Immune");
        if (cardStatus.IsImmue)
        {
            TagsTextMesh.text += string.IsNullOrWhiteSpace(TagsTextMesh.text) ? immuneTag : $", {immuneTag}";
        }
        var doomedTag = translator.GetText("CardTag_Doomed");
        if (cardStatus.IsDoomed && !TagsTextMesh.text.Contains(doomedTag))
        {
            TagsTextMesh.text += string.IsNullOrWhiteSpace(TagsTextMesh.text) ? doomedTag : $", {doomedTag}";
        }
        //Content.sizeDelta = new Vector2(Content.sizeDelta.x, CardInfoText.preferredHeight + 115);
        Content.sizeDelta = new Vector2(Content.sizeDelta.x, CardInfoTextMesh.preferredHeight + 115);
    }
}
