using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

public class DatePicker : MonoBehaviour
{
    [Header("UI References")]
    public GameObject datepickerPanel;
    public TMP_Text monthYearText;
    public Button prevMonthButton;
    public Button nextMonthButton;
    public Transform daysContainer;
    public GameObject dayButtonPrefab;
    public TMP_Text selectedDateText;

    [Header("Settings")]
    public Color normalDayColor = Color.white;
    public Color selectedDayColor = Color.cyan;
    public Color todayColor = Color.yellow;
    public Color otherMonthColor = Color.gray;

    [Header("Events")]
    public UnityEvent OnDateChanged;  

    private DateTime currentMonth;
    private DateTime selectedDate;

    private List<Button> dayButtons = new List<Button>();

    private int Clickcount = 0;

    void Start()
    {
        currentMonth = DateTime.Now;
        selectedDate = DateTime.Now;

        prevMonthButton.onClick.AddListener(PreviousMonth);
        nextMonthButton.onClick.AddListener(NextMonth);
        UpdateCalendar();
        UpdateSelectedDateDisplay();
    }
    
    public void SetActive()
    {
        datepickerPanel.SetActive(true);
    }

    public void ToggleDatepicker()
    {
        datepickerPanel.SetActive(!datepickerPanel.activeSelf);
        if (datepickerPanel.activeSelf)
        {
            currentMonth = selectedDate;
            UpdateCalendar();
        }
    }

    void PreviousMonth()
    {
        currentMonth = currentMonth.AddMonths(-1);
        UpdateCalendar();
    }

    void NextMonth()
    {
        currentMonth = currentMonth.AddMonths(1);
        UpdateCalendar();
    }

    void UpdateCalendar()
    {
        monthYearText.text = currentMonth.ToString("MMMM yyyy");

        // Clear existing buttons
        foreach (Button btn in dayButtons)
        {
            Destroy(btn.gameObject);
        }
        dayButtons.Clear();

        // Get first day of month
        DateTime firstDay = new DateTime(currentMonth.Year, currentMonth.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);
        int startDayOfWeek = (int)firstDay.DayOfWeek;

        // Get days from previous month
        DateTime prevMonth = currentMonth.AddMonths(-1);
        int daysInPrevMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);

        int totalCells = 42; // 6 rows x 7 days
        
        for (int i = 0; i < totalCells; i++)
        {
            GameObject dayObj = Instantiate(dayButtonPrefab, daysContainer);
            Button btn = dayObj.GetComponent<Button>();
            TMP_Text dayText = dayObj.GetComponentInChildren<TMP_Text>();
            Image img = dayObj.GetComponent<Image>();

            DateTime cellDate;
            bool isCurrentMonth = false;

            if (i < startDayOfWeek)
            {
                // Previous month
                int day = daysInPrevMonth - (startDayOfWeek - i - 1);
                cellDate = new DateTime(prevMonth.Year, prevMonth.Month, day);
                img.color = otherMonthColor;
            }
            else if (i < startDayOfWeek + daysInMonth)
            {
                // Current month
                int day = i - startDayOfWeek + 1;
                cellDate = new DateTime(currentMonth.Year, currentMonth.Month, day);
                isCurrentMonth = true;
                img.color = normalDayColor;

                // Highlight today
                if (cellDate.Date == DateTime.Now.Date)
                {
                    img.color = todayColor;
                }

                // Highlight selected date
                if (cellDate.Date == selectedDate.Date)
                {
                    img.color = selectedDayColor;
                }
            }
            else
            {
                // Next month
                int day = i - (startDayOfWeek + daysInMonth) + 1;
                DateTime nextMonth = currentMonth.AddMonths(1);
                cellDate = new DateTime(nextMonth.Year, nextMonth.Month, day);
                img.color = otherMonthColor;
            }

            dayText.text = cellDate.Day.ToString();
            
            DateTime dateCopy = cellDate;
            btn.onClick.AddListener(() => SelectDate(dateCopy));

            dayButtons.Add(btn);
        }
    }

    void SelectDate(DateTime date)
    {
        Clickcount++;
        if(Clickcount >= 2) datepickerPanel.SetActive(false);
        selectedDate = date;
        UpdateSelectedDateDisplay();
        UpdateCalendar();
        SaveSelectedDateToPlayerPrefs();
        
        OnDateChanged?.Invoke();
        
        // Trigger event or callback here if needed
        OnDateSelected(selectedDate);
    }

    void UpdateSelectedDateDisplay()
    {
        if (selectedDateText != null)
        {
            selectedDateText.text = selectedDate.ToString("dd/MM/yyyy");
        }
    }

    void SaveSelectedDateToPlayerPrefs()
    {
        // บันทึกในรูปแบบ MySQL (YYYY-MM-DD)
        string dateForMySQL = selectedDate.ToString("yyyy-MM-dd");
        PlayerPrefs.SetString("selected_date", dateForMySQL);
        PlayerPrefs.Save();  // ⭐ เพิ่ม Save
        Debug.Log("✓ Saved date to PlayerPrefs: " + dateForMySQL);
    }

    // Override this method or add UnityEvent to handle date selection
    protected virtual void OnDateSelected(DateTime date)
    {
        Debug.Log("Selected date: " + date.ToString("dd/MM/yyyy"));
    }

    public DateTime GetSelectedDate()
    {
        return selectedDate;
    }

    public void SetDate(DateTime date)
    {
        selectedDate = date;
        currentMonth = date;
        UpdateCalendar();
        UpdateSelectedDateDisplay();
        OnDateChanged?.Invoke();  // ⭐ เรียก Event
    }

    public string GetSelectedDateForMySQL()
    {
        return selectedDate.ToString("yyyy-MM-dd");
    }
}