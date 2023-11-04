using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDK3.Image;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class ActivityCalendarPanel : UdonSharpBehaviour
{
    [Header("图片轮换时间间隔")]
    [SerializeField] private float LoadDuration = 5f;
    [Header("活动信息URL")]
    [SerializeField] private VRCUrl url;
    [Header("活动图片URL")]
    [SerializeField] private VRCUrl imageurl;
    [SerializeField] private Renderer materialRend;
    [SerializeField] private Transform RecentActivity;
    [SerializeField] private Transform LongtermActivity;
    [SerializeField] private Transform Activitydetail;
    [SerializeField] private Text timenow;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private Transform[] ActivityPage;
    [SerializeField] private Transform[] ActivityPageText;
    [SerializeField] private Image[] PageButton;
    [SerializeField] private RawImage[] rawImages;

    private float nextLoadTime = 0f;
    private int currentPageIndex = 0;
    private int maxPageIndex;
    private Texture2D downloadedTexture;
    private VRCImageDownloader imageDownloader;
    private IUdonEventReceiver udonEventReceiver;
    DataList _recentactivitys;
    DataList _longtermactivity;
    void Start()
    {
        materialRend = GetComponent<Renderer>();
        downloadedTexture = new Texture2D(2040, 1020);
        imageDownloader = new VRCImageDownloader();
        udonEventReceiver = (IUdonEventReceiver)this;
        timenow.text = Networking.GetNetworkDateTime().ToString("yyyy-MM-dd");
        maxPageIndex = ActivityPage.Length - 1;
        var texInfo = new TextureInfo();
        texInfo.GenerateMipMaps = true;
        imageDownloader.DownloadImage(imageurl, materialRend.material, udonEventReceiver, texInfo);
        VRCStringDownloader.LoadUrl(url, udonEventReceiver);
        nextLoadTime = Time.time;
    }
    void Update()
    {
        if (Time.time - nextLoadTime <= LoadDuration) return;
        scrollbar.value += (float)0.1666666666666667;
        if (scrollbar.value > 1f)
            scrollbar.value = 0f;
        float remainder = (float)(scrollbar.value % 0.1666666666666667);
        // Debug.Log("余数" + remainder);
        if (remainder != 0.0f)
            scrollbar.value -= remainder;
        nextLoadTime = Time.time;
    }
    public void NextPage()
    {
        ActivityPage[currentPageIndex].gameObject.SetActive(false); // 关闭当前页
        ActivityPageText[currentPageIndex].gameObject.SetActive(false); // 关闭当前页

        currentPageIndex = (currentPageIndex + 1) % ActivityPage.Length; // 增加页码，并循环到第一页

        ActivityPage[currentPageIndex].gameObject.SetActive(true); // 打开下一页
        ActivityPageText[currentPageIndex].gameObject.SetActive(true); // 打开下一页
    }

    public void PreviousPage()
    {
        ActivityPage[currentPageIndex].gameObject.SetActive(false); // 关闭当前页
        ActivityPageText[currentPageIndex].gameObject.SetActive(false); // 关闭当前页

        currentPageIndex = (currentPageIndex + ActivityPage.Length - 1) % ActivityPage.Length; // 减小页码，并循环到最后一页

        ActivityPage[currentPageIndex].gameObject.SetActive(true); // 打开上一页
        ActivityPageText[currentPageIndex].gameObject.SetActive(true); // 打开上一页
    }

    public void ChangeButtonColor()
    {
        float value = scrollbar.value;
        for (int i = 0; i < PageButton.Length; i++)
        {
            Image image = PageButton[i];
            if (value >= i / (float)PageButton.Length && value <= ((i + 1) / (float)PageButton.Length))
            {
                image.color = new Color(0.992f, 0.561f, 0.322f);
            }
            else
            {
                image.color = Color.white;
            }
        }
    }
    void GenerateRecentActivityView()
    {
        Debug.Log("在读取近期活动");
        for (int i = 0; i < RecentActivity.childCount; i++)
        {
            RecentActivity.GetChild(i).gameObject.SetActive(false);
            if (i >= _recentactivitys.Count) break;
            if (_recentactivitys.TryGetValue(i, TokenType.DataDictionary, out var v))
            {
                Text year = RecentActivity.GetChild(i).Find("year").GetComponentInChildren<Text>();
                Text date = RecentActivity.GetChild(i).Find("date").GetComponentInChildren<Text>();
                Text week = RecentActivity.GetChild(i).Find("week").GetComponentInChildren<Text>();
                Text starttime = RecentActivity.GetChild(i).Find("starttime").GetComponentInChildren<Text>();
                Text endtime = RecentActivity.GetChild(i).Find("endtime").GetComponentInChildren<Text>();
                Text title = RecentActivity.GetChild(i).Find("title").GetComponentInChildren<Text>();
                Text brief = RecentActivity.GetChild(i).Find("brief").GetComponentInChildren<Text>();
                if (v.DataDictionary.TryGetValue("year", TokenType.String, out var yearvalue))
                    year.text = yearvalue.String;
                if (v.DataDictionary.TryGetValue("date", TokenType.String, out var datevalue))
                    date.text = datevalue.String;
                if (v.DataDictionary.TryGetValue("week", TokenType.String, out var weekvalue))
                    week.text = weekvalue.String;
                if (v.DataDictionary.TryGetValue("starttime", TokenType.String, out var starttimevalue))
                    starttime.text = starttimevalue.String;
                if (v.DataDictionary.TryGetValue("endtime", TokenType.String, out var endtimevalue))
                    endtime.text = endtimevalue.String;
                if (v.DataDictionary.TryGetValue("title", TokenType.String, out var titlevalue))
                    title.text = titlevalue.String;
                if (v.DataDictionary.TryGetValue("brief", TokenType.String, out var briefvalue))
                    brief.text = briefvalue.String;
                RecentActivity.GetChild(i).gameObject.SetActive(true);
            }
        }
    }
    void GenerateLongtermActivityView()
    {
        Debug.Log("在读取长期活动");
        for (int i = 0; i < LongtermActivity.childCount; i++)
        {
            LongtermActivity.GetChild(i).gameObject.SetActive(false);
            if (i >= _longtermactivity.Count) break;
            if (_longtermactivity.TryGetValue(i, TokenType.DataDictionary, out var v))
            {
                Text year = LongtermActivity.GetChild(i).Find("year").GetComponentInChildren<Text>();
                Text startdate = LongtermActivity.GetChild(i).Find("startdate").GetComponentInChildren<Text>();
                Text starttime = LongtermActivity.GetChild(i).Find("starttime").GetComponentInChildren<Text>();
                Text enddate = LongtermActivity.GetChild(i).Find("enddate").GetComponentInChildren<Text>();
                Text endtime = LongtermActivity.GetChild(i).Find("endtime").GetComponentInChildren<Text>();
                Text title = LongtermActivity.GetChild(i).Find("title").GetComponentInChildren<Text>();
                Text brief = LongtermActivity.GetChild(i).Find("brief").GetComponentInChildren<Text>();
                if (v.DataDictionary.TryGetValue("year", TokenType.String, out var yearvalue))
                    year.text = yearvalue.String;
                if (v.DataDictionary.TryGetValue("startdate", TokenType.String, out var startdatevalue))
                    startdate.text = startdatevalue.String;
                if (v.DataDictionary.TryGetValue("starttime", TokenType.String, out var starttimevalue))
                    starttime.text = starttimevalue.String;
                if (v.DataDictionary.TryGetValue("enddate", TokenType.String, out var enddatevalue))
                    enddate.text = enddatevalue.String;
                if (v.DataDictionary.TryGetValue("endtime", TokenType.String, out var endtimevalue))
                    endtime.text = endtimevalue.String;
                if (v.DataDictionary.TryGetValue("title", TokenType.String, out var titlevalue))
                    title.text = titlevalue.String;
                if (v.DataDictionary.TryGetValue("brief", TokenType.String, out var briefvalue))
                    brief.text = briefvalue.String;
                LongtermActivity.GetChild(i).gameObject.SetActive(true);
            }
        }
    }
    public void RecentActivityDetail(int index)
    {
        Debug.Log("在读取近期活动详细内容");
        // if (index >= _recentactivitys.Count) break;
        if (_recentactivitys.TryGetValue(index, TokenType.DataDictionary, out var v))
        {
            Text title = Activitydetail.Find("title").GetComponentInChildren<Text>();
            Text time = Activitydetail.Find("time").GetComponentInChildren<Text>();
            Text initiator = Activitydetail.Find("initiator").GetComponentInChildren<Text>();
            Text detail = Activitydetail.Find("detail").GetComponentInChildren<Text>();
            if (v.DataDictionary.TryGetValue("title", TokenType.String, out var titlevalue))
                title.text = titlevalue.String;
            if (v.DataDictionary.TryGetValue("time", TokenType.String, out var timevalue))
                time.text = timevalue.String;
            if (v.DataDictionary.TryGetValue("initiator", TokenType.String, out var initiatorvalue))
                initiator.text = initiatorvalue.String;
            if (v.DataDictionary.TryGetValue("detail", TokenType.String, out var detailvalue))
                detail.text = detailvalue.String;
        }

    }
    public void LongtermActivityDetail(int index)
    {
        Debug.Log("在读取长期活动详细内容");
        // if (index >= _longtermactivity.Count) break;
        if (_longtermactivity.TryGetValue(index, TokenType.DataDictionary, out var v))
        {
            Text title = Activitydetail.Find("title").GetComponentInChildren<Text>();
            Text time = Activitydetail.Find("time").GetComponentInChildren<Text>();
            Text initiator = Activitydetail.Find("initiator").GetComponentInChildren<Text>();
            Text detail = Activitydetail.Find("detail").GetComponentInChildren<Text>();
            if (v.DataDictionary.TryGetValue("title", TokenType.String, out var titlevalue))
                title.text = titlevalue.String;
            if (v.DataDictionary.TryGetValue("time", TokenType.String, out var timevalue))
                time.text = timevalue.String;
            if (v.DataDictionary.TryGetValue("initiator", TokenType.String, out var initiatorvalue))
                initiator.text = initiatorvalue.String;
            if (v.DataDictionary.TryGetValue("detail", TokenType.String, out var detailvalue))
                detail.text = detailvalue.String;
        }

    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        string ApiBase = result.Result;
        Debug.Log($"[<color=#ff70ab>StringLoad</color>] 下载字符串成功 内容为 " + ApiBase);
        if (!VRCJson.TryDeserializeFromJson(result.Result, out var json))
        {
            Debug.Log($"[<color=#ff70ab>StringtoJson</color>] 解析json字符串失败. {json.ToString()}");
            return;
        }
        Debug.Log($"json" + json.TokenType);
        if (json.TokenType != TokenType.DataDictionary) return;
        if (json.DataDictionary.TryGetValue("RecentActivity", out var RecentActivityValue))
        {
            Debug.Log($"RecentActivityalue" + RecentActivityValue.TokenType);
            _recentactivitys = RecentActivityValue.DataList;
        }
        if (json.DataDictionary.TryGetValue("LongtermActivity", out var LongtermActivityValue))
        {
            Debug.Log($"LongtermActivityValue" + LongtermActivityValue.TokenType);
            _longtermactivity = LongtermActivityValue.DataList;
        }
        GenerateRecentActivityView();
        GenerateLongtermActivityView();
    }
    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.Log($"[<color=#ff70ab>StringLoad</color>] 下载字符串失败. ");
    }

    public override void OnImageLoadSuccess(IVRCImageDownload result)
    {
        downloadedTexture = result.Result;
        foreach (var image in rawImages)
        {
            image.texture = downloadedTexture;
        }

    }
    public override void OnImageLoadError(IVRCImageDownload result)
    {
        Debug.Log($"Image not loaded: {result.Error.ToString()}: {result.ErrorMessage}.");
    }
    public void Refresh()
    {
        var texInfo = new TextureInfo();
        texInfo.GenerateMipMaps = true;
        imageDownloader.DownloadImage(imageurl, materialRend.material, udonEventReceiver, texInfo);
        VRCStringDownloader.LoadUrl(url, udonEventReceiver);
    }
}
