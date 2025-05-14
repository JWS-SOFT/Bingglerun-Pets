using UnityEngine;

public class AudioInputTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.Alpha1))
        if (InputManager.Instance.IsZone1Pressed)
        {
            SoundEvents.OnPlaySFX?.Invoke(SFXType.Jump);
            //AudioManager.Instance.PlaySFX(SFXType.Jump);
        }

        //if(Input.GetKeyDown(KeyCode.Alpha2))
        if (InputManager.Instance.IsZone2Pressed)
        {
            SoundEvents.OnPlaySFX?.Invoke(SFXType.Click);
            //AudioManager.Instance.PlaySFX(SFXType.Click);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SoundEvents.OnPlayBGM?.Invoke(BGMType.Title);
            //AudioManager.Instance.PlayBGM(BGMType.Title);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SoundEvents.OnPlayBGM?.Invoke(BGMType.Lobby);
            //AudioManager.Instance.PlayBGM(BGMType.Lobby);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SoundEvents.OnStopBGM.Invoke();
            //AudioManager.Instance.StopBGM();
        }
    }
}
