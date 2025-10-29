using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DatePicker : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject datePickerPanel;
    [SerializeField] private TextMeshProUGUI monthYearText;
    [SerializeField] private TextMeshProUGUI selectedDateText;
    [SerializeField] private Transform daysContainer;
    [SerializeField] private GameObject dayButtonPrefab;

    [Header("Navigation Buttons")]
    [SerializeField] private Button prevMonthButton;
    [SerializeField] private Button nextMonthButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button openPickerButton;

    [Header("Settings")]
    [SerializeField] private Color selectedDayColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color todayColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color normalDayColor = Color.white;
    [SerializeField] private Color otherMonthColor = new Color(0.5f, 0.5f, 0.5f);

    // State
    private DateTime currentDate;
    private DateTime selectedDate;
    private DateTime displayMonth;
    private List<GameObject> dayButtons = new List<GameObject>();
    private GameObject currentSelectedButton;

    // Events
    public event Action<DateTime> OnDateSelected;
    public event Action<DateTime> OnDateConfirmed;

    void Start()
    {
        currentDate = DateTime.Now;
        selectedDate = DateTime.Now;
        displayMonth = new DateTime(currentDate.Year, currentDate.Month, 1);

        SetupButtons();
        datePickerPanel.SetActive(false);
        UpdateSelectedDateDisplay();
    }

    void SetupButtons()
    {
        if (prevMonthButton != null)
            prevMonthButton.onClick.AddListener(PreviousMonth);

        if (nextMonthButton != null)
            nextMonthButton.onClick.AddListener(NextMonth);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmDate);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(CloseDatePicker);

        if (openPickerButton != null)
            openPickerButton.onClick.AddListener(OpenDatePicker);
    }

    // ===== Open/Close =====

    public void OpenDatePicker()
    {
        datePickerPanel.SetActive(true);
        displayMonth = new DateTime(selectedDate.Year, selectedDate.Month, 1);
        GenerateCalendar();
    }

    public void CloseDatePicker()
    {
        datePickerPanel.SetActive(false);
    }

    // ===== Calendar Generation =====

    void GenerateCalendar()
    {
        ClearDayButtons();

        // อัพเดทชื่อเดือน-ปี
        if (monthYearText != null)
        {
            monthYearText.text = displayMonth.ToString("MMMM yyyy");
        }

        // หาวันแรกของเดือน
        DateTime firstDay = new DateTime(displayMonth.Year, displayMonth.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(displayMonth.Year, displayMonth.Month);
        
        // หาว่าวันแรกตรงกับวันอะไร (0=Sunday, 1=Monday, ...)
        int startDayOfWeek = (int)firstDay.DayOfWeek;

        // เพิ่มวันว่างก่อนวันที่ 1
        DateTime prevMonth = displayMonth.AddMonths(-1);
        int daysInPrevMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
        
        for (int i = startDayOfWeek - 1; i >= 0; i--)
        {
            int day = daysInPrevMonth - i;
            CreateDayButton(day, prevMonth, true);
        }

        // เพิ่มวันในเดือนปัจจุบัน
        for (int day = 1; day <= daysInMonth; day++)
        {
            DateTime date = new DateTime(displayMonth.Year, displayMonth.Month, day);
            CreateDayButton(day, date, false);
        }

        // เพิ่มวันของเดือนถัดไป (ถ้ายังไม่ครบ 42 วัน - 6 สัปดาห์)
        int totalButtons = startDayOfWeek + daysInMonth;
        int remainingDays = 42 - totalButtons;
        DateTime nextMonth = displayMonth.AddMonths(1);
        
        for (int day = 1; day <= remainingDays; day++)
        {
            CreateDayButton(day, nextMonth, true);
        }
    }

    void CreateDayButton(int day, DateTime date, bool isOtherMonth)
    {
        if (dayButtonPrefab == null || daysContainer == null)
            return;

        GameObject buttonObj = Instantiate(dayButtonPrefab, daysContainer);
        dayButtons.Add(buttonObj);

        Button button = buttonObj.GetComponent<Button>();
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        Image buttonImage = buttonObj.GetComponent<Image>();

        if (buttonText != null)
        {
            buttonText.text = day.ToString();
        }

        // กำหนดสี
        Color buttonColor = normalDayColor;

        if (isOtherMonth)
        {
            buttonColor = otherMonthColor;
        }
        else if (IsSameDay(date, currentDate))
        {
            buttonColor = todayColor;
        }

        if (buttonImage != null)
        {
            buttonImage.color = buttonColor;
        }

        // เช็คว่าเป็นวันที่เลือกไว้หรือไม่
        if (!isOtherMonth && IsSameDay(date, selectedDate))
        {
            if (buttonImage != null)
            {
                buttonImage.color = selectedDayColor;
            }
            currentSelectedButton = buttonObj;
        }

        // เพิ่ม Event
        if (button != null)
        {
            DateTime capturedDate = date;
            button.onClick.AddListener(() => OnDayClicked(capturedDate, buttonObj));

            // ปิดการกดถ้าเป็นวันของเดือนอื่น
            button.interactable = !isOtherMonth;
        }
    }

    void ClearDayButtons()
    {
        foreach (var btn in dayButtons)
        {
            if (btn != null)
                Destroy(btn);
        }
        dayButtons.Clear();
        currentSelectedButton = null;
    }

    // ===== Event Handlers =====

    void OnDayClicked(DateTime date, GameObject buttonObj)
    {
        // รีเซ็ตปุ่มเก่า
        if (currentSelectedButton != null)
        {
            Image oldImage = currentSelectedButton.GetComponent<Image>();
            if (oldImage != null)
            {
                if (IsSameDay(date, currentDate))
                    oldImage.color = todayColor;
                else
                    oldImage.color = normalDayColor;
            }
        }

        // เลือกปุ่มใหม่
        selectedDate = date;
        currentSelectedButton = buttonObj;

        Image newImage = buttonObj.GetComponent<Image>();
        if (newImage != null)
        {
            newImage.color = selectedDayColor;
        }

        UpdateSelectedDateDisplay();
        OnDateSelected?.Invoke(selectedDate);

        Debug.Log($"Selected Date: {selectedDate:yyyy-MM-dd}");
    }

    void PreviousMonth()
    {
        displayMonth = displayMonth.AddMonths(-1);
        GenerateCalendar();
    }

    void NextMonth()
    {
        displayMonth = displayMonth.AddMonths(1);
        GenerateCalendar();
    }

    void ConfirmDate()
    {
        OnDateConfirmed?.Invoke(selectedDate);
        UpdateSelectedDateDisplay();
        CloseDatePicker();
        Debug.Log($"Confirmed Date: {selectedDate:yyyy-MM-dd}");
    }

    void UpdateSelectedDateDisplay()
    {
        if (selectedDateText != null)
        {
            selectedDateText.text = selectedDate.ToString("dd/MM/yyyy");
        }
    }

    // ===== Helper Methods =====

    bool IsSameDay(DateTime date1, DateTime date2)
    {
        return date1.Year == date2.Year && 
               date1.Month == date2.Month && 
               date1.Day == date2.Day;
    }

    // ===== Public Methods =====

    public void SetDate(DateTime date)
    {
        selectedDate = date;
        displayMonth = new DateTime(date.Year, date.Month, 1);
        UpdateSelectedDateDisplay();
        
        if (datePickerPanel.activeSelf)
        {
            GenerateCalendar();
        }
    }

    public void SetMinDate(DateTime minDate)
    {
        // ใช้สำหรับจำกัดวันที่ต่ำสุดที่เลือกได้
        // TODO: Implement min date validation
    }

    public void SetMaxDate(DateTime maxDate)
    {
        // ใช้สำหรับจำกัดวันที่สูงสุดที่เลือกได้
        // TODO: Implement max date validation
    }

    public DateTime GetSelectedDate()
    {
        return selectedDate;
    }

    public string GetSelectedDateString(string format = "yyyy-MM-dd")
    {
        return selectedDate.ToString(format);
    }

    public void GoToToday()
    {
        SetDate(DateTime.Now);
        if (datePickerPanel.activeSelf)
        {
            GenerateCalendar();
        }
    }

    // ===== Quick Date Selection =====

    public void SelectToday()
    {
        SetDate(DateTime.Now);
    }

    public void SelectYesterday()
    {
        SetDate(DateTime.Now.AddDays(-1));
    }

    public void SelectLastWeek()
    {
        SetDate(DateTime.Now.AddDays(-7));
    }

    public void SelectLastMonth()
    {
        SetDate(DateTime.Now.AddMonths(-1));
    }
}
