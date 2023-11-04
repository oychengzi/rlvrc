
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class ActivityItem : UdonSharpBehaviour
{
    [SerializeField] private ActivityCalendarPanel activitycalendar;
    void Start()
    {

    }
    public void ShowRecentActivityDetail()
    {
        int index = transform.GetSiblingIndex();
        activitycalendar.RecentActivityDetail(index);
    }
    public void ShowLongtermActivityDetail()
    {
        int index = transform.GetSiblingIndex();
        activitycalendar.LongtermActivityDetail(index);
    }
}
