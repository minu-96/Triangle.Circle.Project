using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 칸이나 버튼이 아닌 빈 공간을 눌렀을 때를 받아 낸다.
/// 화면 맨 뒤에 깔아 두면, 위에 있는 칸·버튼이 먼저 클릭을 가져가고
/// 아무것도 맞지 않은 클릭만 여기로 떨어진다.
/// </summary>
public class BackgroundClickCatcher : MonoBehaviour, IPointerClickHandler
{
    /// <summary>런타임에 라우터가 연결한다.</summary>
    public Action Clicked;

    public void OnPointerClick(PointerEventData eventData) => Clicked?.Invoke();
}
