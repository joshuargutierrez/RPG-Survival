using Invector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[vClassHeader("Mult-Toggle Event", helpBoxText = "Use the method SetToggleOn/Off via Events", openClose = false)]
public class vMultToogleEvent : vMonoBehaviour
{
    [System.Serializable]
    public class Toogle
    {
        public string name;
        [Header("Current Value of the toogle")]
        public bool value;
        [Header("Validation to compare with value")]
        public bool validation;

        public void ToogleOn()
        {
            value = true;
        }
        public void ToogleOff()
        {
            value = false;
        }
        public bool isValid => value.Equals(validation);
    }
    public List<Toogle> toogles;
    public bool isValid;

    public UnityEngine.Events.UnityEvent onValidate, onInvalidate;

    public void Start()
    {
        CheckValidation();
    }
    public void ToogleOn(int index)
    {
        if (toogles.Count > 0 && index < toogles.Count)
        {
            toogles[index].ToogleOn();
            CheckValidation();
        }
    }
    public void ToogleOff(int index)
    {
        if (toogles.Count > 0 && index < toogles.Count)
        {
            toogles[index].ToogleOff();
            CheckValidation();
        }
    }

    public void ToogleOn(string name)
    {
        var toogle = toogles.Find(t => t.name.Equals(name));
        if (toogle != null)
        {
            toogle.ToogleOn();
            CheckValidation();
        }
    }

    public void ToogleOff(string name)
    {
        var toogle = toogles.Find(t => t.name.Equals(name));
        if (toogle != null)
        {
            toogle.ToogleOff();
            CheckValidation();
        }
    }

    void CheckValidation()
    {
        var _isValid = isValid;
        var validToogles = toogles.FindAll(t => t.isValid);
        if (validToogles.Count == toogles.Count)
        {
            _isValid = true;
        }
        else
        {
            _isValid = false;
        }

        if (_isValid != isValid)
        {
            isValid = _isValid;
            if (isValid)
            {
                onValidate.Invoke();
            }
            else
            {
                onInvalidate.Invoke();
            }
        }
    }
}
