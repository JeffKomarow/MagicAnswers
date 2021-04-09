using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIHandler : MonoBehaviour
{

    ArrayList leaderBoard = new ArrayList();

    private const int _maxScores = 15;
    private string _email = "";
    private int _score = 100;
    private DateTime _dateTime;
    private string _question;
    private string _answer;

    const int kMaxLogSize = 16382;
    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    protected bool isFirebaseInitialized = false;

    // adding in variables
    public TMP_Text logTextUI;
    public TMP_Text emailUI;
    public TMP_Text scoreUI;

    public TMP_InputField questionInputField;

    public Transform leaderBoardArea;
    public GameObject rowPreFab;



    // At start, check for the required dependencies to use Firebase, and if not, add them if possible.
    protected virtual void Start()
    {
        leaderBoard.Clear();
        leaderBoard.Add("Firebase Top " + _maxScores.ToString() + " Scores");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
                Debug.Log("Setting up Firebase Auth");
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    // Initialize the Firebase database:
    protected virtual void InitializeFirebase()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;
        StartListener();
        isFirebaseInitialized = true;
        Debug.Log("Firebase Initialized");
        logTextUI.text = "Firebase Initialized";
    }

    protected void StartListener()
    {
        FirebaseDatabase.DefaultInstance
          .GetReference("Leaders").OrderByChild("score")
          .ValueChanged += (object sender2, ValueChangedEventArgs e2) =>
          {
              if (e2.DatabaseError != null)
              {
                  Debug.Log(e2.DatabaseError.Message);
                  return;
              }
              Debug.Log("Received values for Leaders.");
              string title = leaderBoard[0].ToString();
              leaderBoard.Clear();
              leaderBoard.Add(title);
              if (e2.Snapshot != null && e2.Snapshot.ChildrenCount > 0)
              {
                  foreach (var childSnapshot in e2.Snapshot.Children)
                  {
                      if (childSnapshot.Child("question") == null || childSnapshot.Child("answer") == null)
                      {
                          Debug.Log("Bad data in sample.");
                          break;
                      }
                      else
                      {
                    //      Debug.Log("Leaders entry : " +
                    //childSnapshot.Child("question").Value.ToString() + " - " +
                    //childSnapshot.Child("answer").Value.ToString());
                          leaderBoard.Insert(1, childSnapshot.Child("question").ToString() + "  " + childSnapshot.Child("answer").ToString());

                          GameObject tempGo = Instantiate(rowPreFab, leaderBoardArea);
                          TMP_Text[] texts = tempGo.GetComponentsInChildren<TMP_Text>();
                          texts[0].text = childSnapshot.Child("question").Value.ToString();
                          texts[1].text = childSnapshot.Child("answer").Value.ToString();
                          texts[2].text = childSnapshot.Child("datetime").Value.ToString();
                      }
                  }
              }
          };
    }

    // Exit if escape (or back, on mobile) is pressed.
    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }


    // A realtime database transaction receives MutableData which can be modified
    // and returns a TransactionResult which is either TransactionResult.Success(data) with
    // modified data or TransactionResult.Abort() which stops the transaction with no changes.
    TransactionResult AddScoreTransaction(MutableData mutableData)
    {
        List<object> leaders = mutableData.Value as List<object>;

        if (leaders == null)
        {
            leaders = new List<object>();
        }
        //else if (mutableData.ChildrenCount >= _maxScores)
        //{
        //    // If the current list of scores is greater or equal to our maximum allowed number,
        //    // we see if the new score should be added and remove the lowest existing score.
        //    long minScore = long.MaxValue;
        //    object minVal = null;
        //    foreach (var child in leaders)
        //    {
        //        if (!(child is Dictionary<string, object>))
        //            continue;
        //        long childScore = (long)((Dictionary<string, object>)child)["score"];
        //        if (childScore < minScore)
        //        {
        //            minScore = childScore;
        //            minVal = child;
        //        }
        //    }
        //    // If the new score is lower than the current minimum, we abort.
        //    if (minScore > _score)
        //    {
        //        return TransactionResult.Abort();
        //    }
        //    // Otherwise, we remove the current lowest to be replaced with the new score.
        //    leaders.Remove(minVal);
        //}

        // Now we add the new score as a new entry that contains the email address and score.
        Dictionary<string, object> newScoreMap = new Dictionary<string, object>();
        newScoreMap["question"] = _question;
        newScoreMap["answer"] = _answer;
        newScoreMap["datetime"] = DateTime.Now.ToString();
        leaders.Add(newScoreMap);

        // You must set the Value to indicate data at that location has changed.
        mutableData.Value = leaders;
        return TransactionResult.Success(mutableData);
    }

    public void AddQuestion(string question, string answer)
    {
        _question = question;
        _answer = answer;
        if (string.IsNullOrEmpty(question) || string.IsNullOrEmpty(answer))
        {
            Debug.Log("invalid question.");
            logTextUI.text = "invalid question.";
            return;
        }
        Debug.Log(question + " " + answer);
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("Leaders");

        Debug.Log("Running Transaction...");
        // Use a transaction to ensure that we do not encounter issues with
        // simultaneous updates that otherwise might create more than MaxScores top scores.
        reference.RunTransaction(AddScoreTransaction)
          .ContinueWithOnMainThread(task =>
          {
              if (task.Exception != null)
              {
                  Debug.Log(task.Exception.ToString());
              }
              else if (task.IsCompleted)
              {
                  Debug.Log("Transaction complete.");
              }
          });
    }

    //_email = TextField.text(email);

    //public void UpdateScoreUI()
    //{
    //    scoreUI.text("Score:", 0));
    //    int.TryParse(scoreUI.TextField(_score.ToString()), out _score);
    //}


    public void AuthenticateScore()
    {
        AddQuestion(questionInputField.text, MagicAnswer.AskAQuestion());
    }

    public void GoOfflineButton()
    {
        FirebaseDatabase.DefaultInstance.GoOffline();
        Debug.Log("You are Offline");
        logTextUI.text = "You are Offline";
    }

    public void GoOnlineButton()
    {
        FirebaseDatabase.DefaultInstance.GoOnline();
        Debug.Log("You are online");
        logTextUI.text = "You are Online";

        //void DisplayLeaders()
        //foreach (string item in leaderBoard)

    }




    //experimental

    //public void writeNewUser(string email, string name, string pass, int score)
    //{
    //    Player user = new Player(emailRegisterField.text, usernameRegisterField.text, passwordRegisterField.text, PlayerPrefs.GetInt("HighScore"));
    //    string json = JsonUtility.ToJson(user);
    //    databaseReference.Child("users").Push().SetRawJsonValueAsync(json);
    //}


    //public void WriteNewScore(string userId, int score)
    //{
    //    // Create new entry at /user-scores/$userid/$scoreid and at
    //    // /leaderboard/$scoreid simultaneously
    //    string key = databaseReference.Child(userId)/*.Push()*/.Key;
    //    LeaderboardEntry entry = new LeaderboardEntry(userId, score);
    //    Dictionary<string, object> entryValues = entry.ToDictionary();

    //    Dictionary<string, object> childUpdates = new Dictionary<string, object>();
    //    //childUpdates["/scores/" + key] = entryValues;
    //    childUpdates["/Highscore/" /*+ userId*/ + "/" + key] = entryValues;

    //    databaseReference.UpdateChildrenAsync(childUpdates);
    //}
}