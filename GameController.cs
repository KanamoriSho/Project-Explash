using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{

    [System.Serializable]
    public class ObjList
    {
        public List<GameObject> objList = new List<GameObject>(5);//GameObject型のリスト
    }
    [System.Serializable]
    public class IntList
    {
        public List<int> intList = new List<int>(5);//int型のリスト
    }
    [System.Serializable]
    public class StringList
    {
        public List<string> stringList = new List<string>(5);//string型のリスト
    }
    [System.Serializable]
    public class UnitList
    {
        public List<Unit> objList = new List<Unit>(5);//Unit型のリスト
    }

    [Header("プレイヤー")]
    [SerializeField]
    public ObjList[] players = new ObjList[2];

    [Header("ユニット")]
    [SerializeField]
    public UnitList[] units = new UnitList[2];

    [Header("名前")]
    [SerializeField]
    public StringList[] names = new StringList[2];

    [Header("スコア")]
    [SerializeField]
    public IntList[] scores = new IntList[2];

    [Header("キル数")]
    [SerializeField]
    public IntList[] kills = new IntList[2];

    [Header("デス数")]
    [SerializeField]
    public IntList[] deathes = new IntList[2];

    [Header("拠点")]
    [SerializeField]
    public ObjList[] bases = new ObjList[2];

    [Header("与ダメージ")]
    [SerializeField]
    public IntList[] damages = new IntList[2];

    [Header("拠点破壊アイコン")]
    [SerializeField]
    public ObjList[] destroyIcons = new ObjList[2];

    //プレイヤーのオブジェクト
    public GameObject playerObj;

    //プレイヤーのUnitコンポーネント
    public Unit player;

    [SerializeField,Header("デフォルトカメラ")]
    private GameObject defaultCamera;

    [SerializeField, Header("イントロカメラ")]
    private GameObject introCamera;

    [SerializeField, Header("上下黒枠")]
    private GameObject blackLater = null;

    private Animator blackLaterAnim = null;

    [SerializeField, Header("スタート前待機時間")]
    private int waitTimeBeforeStart = default;

    [SerializeField, Header("UI表示待機時間")]
    private int waitTimeBeforeStart_UI = default;

    [SerializeField,Header("カウントダウン秒数")]
    private const int START_WAIT_TIME = 3;

    //カウントダウン秒数(計算用)
    [System.NonSerialized]
    public float startCountDown;

    [SerializeField,Header("終了時の待ち時間")]
    private int finishWaitTime;

    [SerializeField, Header("試合時間(秒)")]
    private float maxTime = 300;

    //残り時間(秒)
    [System.NonSerialized]
    public float time = 0;

    //一分の秒数(定数)
    private const int ONE_MINUTE = 60;

    //残り時間(分)
    [System.NonSerialized]
    public int minute = 0;

    [SerializeField, Header("スタート時のUI")]
    private GameObject StartUI;

    [SerializeField, Header("メインUI")]
    private GameObject MainUI;

    [SerializeField, Header("メインUI")]
    private GameObject MainUI2;

    [SerializeField, Header("ポーズUI")]
    private GameObject PauseUI;

    [SerializeField,Header("リザルトUI")]
    private GameObject ResultUI;

    //UIのAnimator
    private Animator UI_Animator;

    [SerializeField,Header("ポーズ中フラグ")]
    public bool _inPause = false;

    //味方チームのスコア(UI表示用)
    public float prePlayerScore = 0;

    //敵チームのスコア(UI表示用)
    public float preEnemyScore = 0;

    //味方チームのスコア
    public int playerScore = 0;

    //敵チームのスコア
    public int enemyScore = 0;

    //1チームのプレイヤー数
    private const int TEAM_UNITS = 5;

    //合計プレイヤー数
    private const int TOTAL_UNITS = 10;

    [SerializeField,Header("デモプレイフラグ")]
    public bool _demoPlay = false;

    [SerializeField,Header("カウントダウン音")]
    private AudioClip SE_statSound = null;

    [SerializeField,Header("VibrationController")]
    private VibrationController VC = null;

    [SerializeField,Header("デモ時に消すオブジェクト")]
    private GameObject[] demoFalseObjects;

    [SerializeField,Header("デモ時テキスト")]
    private GameObject demoText;

    private GameObject _fpsObj;// FPS表示オブジェクト

    private enum GameProgress
    {
        beforeStart,//開始前
        inGame,//ゲーム中
        gameover,//ゲームオーバー
    }
    private GameProgress progress;

    public enum GameMode
    {
        Easy,//イージー
        Normal,//ノーマル
        Hard,//ハード
    }
    public int gameMode;

    //ModeControllerを格納
    private ModeController MC;

    //試合中かどうかを返すフラグ
    public bool _inGame()
    {
        //試合中の時のみtrueを返す
        if (progress == GameProgress.inGame)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Awake()//すべてのStartより前に実行する
    {
        //コンポーネント取得、難易度設定
        player = playerObj.GetComponent<Unit>();
        MC = GameObject.FindGameObjectWithTag("ModeController").GetComponent<ModeController>();
        gameMode = MC.difficulty;
        blackLaterAnim = blackLater.GetComponent<Animator>();

        //デモプレイ時の処理
        if (_demoPlay)
        {
            //オブジェクトを消してテキスト表示
            for (int i = 0; i < demoFalseObjects.Length; i++)
            {
                demoFalseObjects[i].SetActive(false);
            }
            demoText.SetActive(true);
            gameMode = (int)GameMode.Normal;
        }
        //fps表示
        _fpsObj = GameObject.Find("FPSText");
    }

    private void Start()
    {
        //UI非アクティブ化
        StartUI.SetActive(false);
        ResultUI.SetActive(false);
        PauseUI.SetActive(false);

        //fps表示を消す
        if (_fpsObj)
        {
            _fpsObj.SetActive(false);
        }
        
        //全プレイヤーの名前を取得
        GetNames();

        //UIのAnimator取得
        UI_Animator = MainUI.GetComponent<Animator>();

        //試合時間を設定
        time = maxTime;

        //試合時間が一分以上なら60で割り、商を分、余りを秒とする
        if (time >= ONE_MINUTE)
        {
            minute = (int)time / ONE_MINUTE;
            time = time % minute;
        }
        //progresを試合前に設定
        progress = GameProgress.beforeStart;

        //試合開始処理
        StartCoroutine(GameStart());

    }


    private void Update()
    {
        //Fが押されたらFPS表示
        if (Input.GetKeyDown(KeyCode.F))
        {
            _fpsObj.SetActive(!_fpsObj.activeSelf);
        }

        //デモプレイなら決定ボタンでタイトルへ
        if (_demoPlay)
        {
            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                SceneManager.LoadScene(0);
            }
        }

        //Escでタイトルへ
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadSceneAsync(0);
        }

        //試合中のみ行う処理
        if (progress == GameProgress.inGame)
        {

            Timer();//タイム計算

            //UI表示用スコアを徐々に実際のスコアに近づける
            prePlayerScore = Mathf.Lerp(prePlayerScore, playerScore, Time.deltaTime * 5);
            preEnemyScore = Mathf.Lerp(preEnemyScore, enemyScore, Time.deltaTime * 5);

            //ポーズボタンが押されたらポーズ切り替え
            //if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame && !_inPause)
            //{
            //    Pause();
            //}
        }

    }
    private IEnumerator GameStart()
    {
        //UI非表示
        MainUI.SetActive(false);

        if (MainUI2 != null)
        {
            MainUI2.SetActive(false);
        }

        //カメラ切り替え
        introCamera.SetActive(true);
        defaultCamera.SetActive(false);

        //イントロ終了まで待つ
        yield return new WaitForSeconds(waitTimeBeforeStart);

        //カウントダウンを設定
        startCountDown = START_WAIT_TIME;

        //UIとカメラ切り替え
        defaultCamera.SetActive(true);
        introCamera.SetActive(false);
        blackLater.SetActive(true); 
        
        yield return new WaitForSeconds(waitTimeBeforeStart_UI);
        MainUI.SetActive(true);

        if (MainUI2 != null)
        {
            MainUI2.SetActive(true);
        }
        introCamera.GetComponent<Camera>().enabled = false;

        //3秒待つ
        yield return new WaitForSeconds(3f);

        //カウントダウンUI表示
        StartUI.SetActive(true);
        
        //カウントダウン処理
        if (SE_statSound)
        {
            StartCoroutine(CountDown());
        }
        //カウントダウン終了を待つ
        while (startCountDown > 0)
        {
            startCountDown -= Time.deltaTime;
            yield return null;
        }
        //カウントダウン終了
        startCountDown = 0;

        //progressを試合中にする
        progress = GameProgress.inGame;

        //チーム数ループ
        for(int teams = 0; teams < players.Length; teams++)
        {
            ///プレイヤー人数分ループ
            for(int playerCount = 0; playerCount < players[teams].objList.Count; playerCount++)
            {
                //アルティメットメーターの自然回復開始
                players[teams].objList[playerCount].GetComponent<UltimateMeter>().GameStart();
            }
        }

        //3秒後にカウントダウンUI非表示
        yield return new WaitForSeconds(3f);
        StartUI.SetActive(false);
    }
    private IEnumerator CountDown()
    {
        //AudioSourceを取得し、カウントダウン音を鳴らす
        AudioSource audioSource = this.GetComponent<AudioSource>();
        audioSource.PlayOneShot(SE_statSound);

        yield return new WaitForSeconds(1);
    }
    /// <summary>
    /// タイム計算処理
    /// </summary>
    private void Timer()
    {
        //試合時間を常に減らす
        time -= Time.deltaTime;

        //秒数が0になったとき
        if (time <= 0)
        {
            //一分減らす
            minute--;

            //60秒足す
             time += ONE_MINUTE;

            //残り時間が0分の場合
            if (minute < 0)
            {
                //試合終了
                minute = 0;
                time = 0;
                StartCoroutine(GameOver());
            }
        }
    }
    public IEnumerator GameOver()
    {
        //試合中なら通らない
        if (!_inGame())
        {
            yield break;
        }

        //振動止める
        VC.ResetVibration();
        VC.ResetVibration2();

        VC.SetIsPlay();

        //progressをゲームオーバーにする
        progress = GameProgress.gameover;

        //不要なUI非表示
        MainUI.SetActive(false);

        if(MainUI2 != null)
        {
            MainUI2.SetActive(false);
        }
        
        PauseUI.SetActive(false);

        //ポーズoff
        _inPause = false;

        //一定時間待つ
        yield return new WaitForSeconds(finishWaitTime);

        blackLaterAnim.SetTrigger("BaseDestroy");
        //デモプレイならタイトルへ
        if (_demoPlay)
        {
            SceneManager.LoadSceneAsync(0);
            yield break;
        }

        //リザルトUI表示
        ResultUI.SetActive(true);
    }

    private void Pause()
    {
        //ポーズ中でなければポーズon、ポーズ中ならポーズoff
        if (_inPause)
        {
            PauseUI.SetActive(false);
            _inPause = false;
        }
        else
        {
            PauseUI.SetActive(true);
            _inPause = true;
        }
    }

    /// <summary>
    /// リスポーン処理(位置、角度、スポーンさせるオブジェクトを引数で受け取る)
    /// </summary>
    public IEnumerator Respawn(float time, Vector3 pos, Vector3 rotation, GameObject obj)
    {
        //スポーン時間を設定
        float spawnTime = time;

        //オブジェクトを非アクティブ化
        obj.SetActive(false);

        //一定時間待つ
        yield return new WaitForSeconds(spawnTime);

        //指定位置に移動
        obj.transform.position = pos;

        //角度を設定
        obj.transform.localEulerAngles = rotation;

        //オブジェクトをアクティブ化
        obj.SetActive(true);

    }

    public void GetNames()
    {
        //全てのUnitから名前を取得し、対応する配列に格納
        for (int i = 0; i < names.Length; i++)
        {
            for (int j = 0; j < names[i].stringList.Count; j++)
            {
                names[i].stringList[j] = units[i].objList[j].GetComponent<Unit>().playerName;
            }
        }

    }

    /// <summary>
    /// スコア加算処理(撃破スコアと番号を引数で受け取る)
    /// </summary>
    public void AddScore(int score, int num)
    {
        //味方側(番号がチーム人数未満)のとき
        if (num < TEAM_UNITS)
        {
            //UIのスコア増加アニメーション
            UI_Animator.SetTrigger("PlayerScore");

            //スコア加算
            prePlayerScore = playerScore;
            playerScore += score;

            //プレイヤーごとのスコア加算
            scores[0].intList[num] += score;
        }
        //敵側(番号がチーム人数以上)のとき
        else if (num > TEAM_UNITS && num < TOTAL_UNITS)
        {
            //UIのスコア増加アニメーション
            UI_Animator.SetTrigger("EnemyScore");

            //スコア加算
            preEnemyScore = enemyScore;
            enemyScore += score;

            //プレイヤーごとのスコア加算
            scores[1].intList[num - TEAM_UNITS] += score;
        }
    }

    public void AddDeath(int num)
    {
        //番号に一致するプレイヤーのデス数を加算
        if (num < TEAM_UNITS)
        {
            deathes[0].intList[num] += 1;
        }
        else if (num < TOTAL_UNITS)
        {
            deathes[1].intList[num - TEAM_UNITS] += 1;
        }


    }
    public void AddKill(int num, string loc_name)
    {
        //番号に一致するプレイヤーのキル数を加算
        if (num < TEAM_UNITS)
        {
            kills[0].intList[num] += 1;
        }
        else if (num < TOTAL_UNITS)
        {
            kills[1].intList[num - TEAM_UNITS] += 1;
        }

        //プレイヤーがキルした場合はキルログ表示
        if (num == 0)
        {
            MainUI.GetComponent<MainUI>().KillLog(loc_name);
        }

    }

    public void BaseDestroyed()
    {
        //拠点が破壊されたらゲームオーバーにする
        time = 0;
        StartCoroutine(GameOver());
    }


    public int GetKillTop(int loc_num)
    {
        //キル数一位を取得

        if (loc_num < TEAM_UNITS)
        {
            int friendryKillMVP = kills[loc_num].intList.IndexOf(kills[loc_num].intList.Max());

            return friendryKillMVP;
        }
        else
        {
            int enemyKillMVP = kills[1].intList.IndexOf(kills[1].intList.Max());

            return enemyKillMVP;
        }
    }

    public void GetDamage(int teamNumber, int playerNumber, int damageTaken)
    {
        damages[teamNumber].intList[playerNumber] += damageTaken;
    }

    public int GetBaseDamageTop(int loc_num)
    {
        //拠点への与ダメージ一位を取得

        if (loc_num < TEAM_UNITS)
        {
            int friendryDamageMVP = damages[loc_num].intList.IndexOf(damages[loc_num].intList.Max());

            return friendryDamageMVP;

            //return units[1].objList[i].gameObject;
        }
        else
        {
            int enemyDamageMVP = damages[1].intList.IndexOf(damages[1].intList.Max());

            return enemyDamageMVP;

            //return units[0].objList[i].gameObject;
        }
    }

    public GameObject GetScoreTop(int loc_num)
    {
        //スコア1位を取得(未使用)

        int i;
        if (loc_num < TEAM_UNITS)
        {
            i = scores[1].intList.IndexOf(kills[1].intList.Max());
            return units[1].objList[i].gameObject;
        }
        else
        {
            i = scores[0].intList.IndexOf(kills[1].intList.Max());
            return units[0].objList[i].gameObject;
        }
    }
}
