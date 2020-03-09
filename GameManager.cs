using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : SingletonMonobehavior<GameManager> {

	public RectTransform PauseImage;
	public RectTransform PauseMenu;
	public RectTransform ScreenMenu;
	public RectTransform SoundMenu;


	public RectTransform InfoMenu;
	public RectTransform statusPanel;
	public RectTransform slotPanel;
	public RectTransform skillPanel;
	public Text statusTitle;
	public Text slotTitle;
	public Text skillTitle;

	[Header("게임오버 관련")]
	public Image gameOverImg;

	[Header("드롭다운 관련")]
	public GameObject _light;
	public Transform DropDown;


	protected override void Awake()
	{
		Screen.SetResolution(2220, 1080, true);
		base.Awake();
	}

	/// <summary>
	/// 안드로이드 , 윈도우 인지 확인
	/// </summary>
	void FixedUpdate () {
		if(Application.platform == RuntimePlatform.Android)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Pause();
			}
		}
		if(Application.platform == RuntimePlatform.WindowsEditor)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Pause();
			}
		}
		if (DropDown.GetComponent<Dropdown>().value == 0)
		{
			_light.GetComponent<Light>().shadows = LightShadows.Soft;
		}
		if (DropDown.GetComponent<Dropdown>().value == 1)
		{
			_light.GetComponent<Light>().shadows = LightShadows.Hard;
		}
		if (DropDown.GetComponent<Dropdown>().value == 2)
		{
			_light.GetComponent<Light>().shadows = LightShadows.None;
		}
	}

	/// <summary>
	/// esc를 눌렀을때 메뉴가 나오도록
	/// </summary>
	public void Pause()
	{
		if (!PauseImage.gameObject.activeInHierarchy)
		{
			if (!PauseMenu.gameObject.activeInHierarchy)
			{
				PauseMenu.gameObject.SetActive(true);
				SoundMenu.gameObject.SetActive(false);
				ScreenMenu.gameObject.SetActive(false);
			}
			PauseImage.gameObject.SetActive(true);
			Time.timeScale = 0f;
		}
		else
		{
			PauseImage.gameObject.SetActive(false);
			Time.timeScale = 1f;
		}
	}


	/// <summary>
	/// 나가기
	/// </summary>
	/// <param name="isQuit"></param>
	public void SaveSettings(bool isQuit)
	{
		if (isQuit)
		{
			Time.timeScale = 1;
			SceneManager.LoadScene("StartScene");
		}

	}


	/// <summary>
	/// 사운드메뉴
	/// </summary>
	/// <param name="Open"></param>
	public void Sounds(bool Open)
	{
		if (Open)
		{
			SoundMenu.gameObject.SetActive(true);
			ScreenMenu.gameObject.SetActive(false);
			PauseMenu.gameObject.SetActive(false);
		}
		else
		{
			SoundMenu.gameObject.SetActive(false);
			PauseMenu.gameObject.SetActive(true);
		}

	}

	/// <summary>
	/// 스크린세팅메뉴
	/// </summary>
	/// <param name="Open"></param>
	public void ScreenSetting(bool Open)
	{
		if (Open)
		{
			SoundMenu.gameObject.SetActive(false);
			ScreenMenu.gameObject.SetActive(true);
			PauseMenu.gameObject.SetActive(false);
		}
		else
		{
			ScreenMenu.gameObject.SetActive(false);
			PauseMenu.gameObject.SetActive(true);
		}
	}
	/// <summary>
	/// 스탯,인벤토리,스킬창 열기
	/// </summary>
	/// <param name="Open"></param>
	public void InfoScreen(bool Open)
	{
		if (!InfoMenu.gameObject.activeInHierarchy)
		{
			if (!statusPanel.gameObject.activeInHierarchy)
			{
				InfoMenu.gameObject.SetActive(true);
			}
		}
		else
		{
			InfoMenu.gameObject.SetActive(false);
		}
	}

	public void ColorChange(int colorsele)
	{
		switch (colorsele)
		{
			case 1://rgb
				statusTitle.fontStyle = FontStyle.Bold;
				slotTitle.fontStyle = FontStyle.Normal;
				skillTitle.fontStyle = FontStyle.Normal;
				break;
			case 2:
				statusTitle.fontStyle = FontStyle.Normal;
				slotTitle.fontStyle = FontStyle.Bold;
				skillTitle.fontStyle = FontStyle.Normal;
				break;
			case 3:
				statusTitle.fontStyle = FontStyle.Normal;
				slotTitle.fontStyle = FontStyle.Normal;
				skillTitle.fontStyle = FontStyle.Bold;
				break;
		}
	}

	/// <summary>
	/// 스탯창열기
	/// </summary>
	/// <param name="open"></param>
	public void StatWindow()
	{
		if (!statusPanel.gameObject.activeInHierarchy)
		{
			statusPanel.gameObject.SetActive(true);
			slotPanel.gameObject.SetActive(false);
			skillPanel.gameObject.SetActive(false);
			ColorChange(1);
		}
	}

	/// <summary>
	/// 인벤토리 열기
	/// </summary>
	public void InfoInventoryWindow()
	{
		if (!slotPanel.gameObject.activeInHierarchy)
		{
			statusPanel.gameObject.SetActive(false);
			slotPanel.gameObject.SetActive(true);
			skillPanel.gameObject.SetActive(false);
			ColorChange(2);
		}
	}

	public void SkillWindow()
	{
		if (!skillPanel.gameObject.activeInHierarchy)
		{
			statusPanel.gameObject.SetActive(false);
			slotPanel.gameObject.SetActive(false);
			skillPanel.gameObject.SetActive(true);
			ColorChange(3);
		}
	}

	/// <summary>
	/// 게임시작씬으로 돌아가기
	/// </summary>
	public void SceneMove()
	{
		if(gameOverImg.color.a < 0.7f)
		{
			return;
		}
		Time.timeScale = 1;
		SceneManager.LoadScene("GameStart");
	}

	/// <summary>
	/// 게임시작씬으로 돌아가기
	/// </summary>
	public void SceneStart()
	{
		Time.timeScale = 1;
		SceneManager.LoadScene("GameStart");
	}

}
