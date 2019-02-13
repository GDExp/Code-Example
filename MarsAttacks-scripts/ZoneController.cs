using System.Collections.Generic;
using UnityEngine;

//класс областей - проврека дистанций, захват, размещение
public class PointInfo
{
    public Vector3 point_position;
    private Transform image;
    private UnitMechanics current_unit;

    //конструктор
    public PointInfo(Transform position)
    {
        point_position = position.position;
        current_unit = null;
    }
    //точка занята?
    public bool GetPointStatus()
    {
        bool booked;
        booked = (current_unit != null) ? true : false;
        return booked;
    }
    public void SetPointImage(Transform ui)
    {
        if (ui == null && image != null)
        {
            image.gameObject.SetActive(false);
            if(current_unit != null)
                current_unit.UnitDead();
        }
        image = ui;
    }
    //занять точку
    public void SetUnitInPoint(UnitMechanics unit)
    {
        current_unit = unit;
    }
}

public class ZoneController : MonoBehaviour
{
    private GameController game_controller;
    private PlayerController player_controller;
    
    private List<Transform> mars_units;
    private List<Transform> earth_units;
    private PointInfo[] zone_points;
    private Transform zone_ui;

    //захват и ресурсы
    private sbyte capture;
    private float timer;

    private void Start()
    {
        game_controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        player_controller = game_controller.PlayerReference();
        mars_units = new List<Transform>();
        earth_units = new List<Transform>();
        SetupZone();
    }

    private void LateUpdate()
    {
        if (timer < Time.time && capture != 0)
        {
            timer = Time.time + 10f;
            player_controller.RefreshGameInterface(2, 10);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Unit"))
        {
            UnitMechanics current_unit = other.GetComponent<UnitMechanics>();
            if (current_unit is EarthUnit)
                earth_units.Add(current_unit.transform);
            else
                mars_units.Add(current_unit.transform);

            current_unit.RegistrationZone(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Unit"))
        {
            UnitMechanics current_unit = other.GetComponent<UnitMechanics>();
            if (current_unit is EarthUnit)
                earth_units.Remove(current_unit.transform);
            else
                mars_units.Remove(current_unit.transform);

            current_unit.RegistrationZone(null);
        }
    }
    
    private void SetupZone()
    {
        capture = 100;
        timer = Time.time + 10f;//первичное получение LP

        Transform points = transform.Find("Points");
        zone_points = new PointInfo[points.childCount];

        for(int i = 0; i< zone_points.Length; ++i)
        {
            zone_points[i] = new PointInfo(points.GetChild(i));
            player_controller.SetupPointUI(zone_points[i]);
        }

        zone_ui = player_controller.ZoneUIReference(name);
    }

    public Rigidbody GetNearUnit(UnitMechanics unit, Vector3 current_position)
    {
        Rigidbody near_position = null;
        List<Transform> current_list;
        
        current_list = (unit is EarthUnit) ? mars_units : earth_units;
        if (current_list.Count == 0) return near_position;

        //работа с листом проверка дистанции
        float current_distance = Mathf.Infinity;
        float distance = 0f;
        RaycastHit hit;

        for (int i = 0; i < current_list.Count; ++i)
        {
            distance = (current_position - current_list[i].localPosition).sqrMagnitude;
            if(distance < current_distance 
                && Physics.Raycast(new Ray(current_position + Vector3.up * 5f, (current_list[i].localPosition - current_position)), out hit, 40f) 
                && hit.collider.CompareTag("Unit"))
            {
                near_position = current_list[i].GetComponent<Rigidbody>();
                current_distance = distance;
            }
        }
        return near_position;
    }

    public void ResetUnitList(UnitMechanics unit)
    {
        if (unit is EarthUnit)
            earth_units.Remove(unit.transform);
        else
            mars_units.Remove(unit.transform);
    }

    public void RefreshCaptureZone(UnitMechanics unit)
    {
        if(unit is MarsUnit && capture != 0)
        {
            if (capture > 0)
                capture -= 5;
            else
                capture = 0;
            if (capture == 0)
            {
                transform.GetChild(0).gameObject.SetActive(false);
                for (int i = 0; i < zone_points.Length; ++i)
                    zone_points[i].SetPointImage(null);
            }
            player_controller.RefreshZoneFlag(zone_ui, (float)capture / 100);
        }
    }
}
