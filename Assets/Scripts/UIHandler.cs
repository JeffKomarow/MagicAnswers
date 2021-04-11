using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


// NOTES
// Load the 25 latest questions asked. Cut off the rest
// Question cooldown not working.
// 


public class UIHandler : MonoBehaviour
{

    ArrayList questions = new ArrayList();

    private const int _maxAnswers = 25;
    private DateTime _dateTime;
    private string _question;
    private string _answer;
    private string _user;

    private bool canAsk = true;

    const int kMaxLogSize = 16382;
    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    protected bool isFirebaseInitialized = false;

    public TMP_Text TextUI;

    public TMP_InputField questionInputField;

    public GameObject content_0;
    public GameObject content_1;
    
    public Transform leaderBoardArea;
    public GameObject rowPreFab;

    

    // At start, check for the required dependencies to use Firebase, and if not, add them if possible.
    protected virtual void Start()
    {
        canAsk = true;
        questions.Clear();
        //questions.Add("Firebase Top " + _maxScores.ToString() + " Scores");


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
        _user = SystemInfo.deviceUniqueIdentifier.ToString();
        _user = _user.Substring(Math.Max(0, _user.Length - 6));
        TextUI.text = "The underworld is ready for your question " + _user + ".";
    }

    protected void StartListener()
    {
        FirebaseDatabase.DefaultInstance
          .GetReference("Questions").OrderByChild("date")
          .ValueChanged += (object sender2, ValueChangedEventArgs e2) =>
          {
              if (e2.DatabaseError != null)
              {
                  Debug.Log(e2.DatabaseError.Message);
                  return;
              }
              Debug.Log("Received questions from the list.");
              string title = questions[0].ToString();
              questions.Clear();
              questions.Add(title);
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
                          questions.Insert(1, childSnapshot.Child("question").ToString() + "  " + childSnapshot.Child("answer").ToString());

                          GameObject tempGo = Instantiate(rowPreFab, leaderBoardArea);
                          TMP_Text[] texts = tempGo.GetComponentsInChildren<TMP_Text>();
                          texts[0].text = childSnapshot.Child("question").Value.ToString();
                          texts[1].text = childSnapshot.Child("answer").Value.ToString();
                          texts[3].text = childSnapshot.Child("user").Value.ToString();
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
    TransactionResult AddQuestion(MutableData mutableData)
    {
        List<object> Answers = mutableData.Value as List<object>;

        if (Answers == null)
        {
            Answers = new List<object>();
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
        Dictionary<string, object> newAnswerMap = new Dictionary<string, object>
        {
            ["question"] = _question,
            ["answer"] = _answer,
            ["user"] = _user,
            ["datetime"] = DateTime.Now.ToString()
        };
        Answers.Add(newAnswerMap);

        // You must set the Value to indicate data at that location has changed.
        mutableData.Value = Answers;
        return TransactionResult.Success(mutableData);
    }

    public void AddQuestion(string question, string answer)
    {
        if (canAsk == true)
        {
            _question = question;
            _answer = answer;
            canAsk = false;
            StartCoroutine(QuestionCooldown());
        }
        if (string.IsNullOrEmpty(question) || string.IsNullOrEmpty(answer))
        {
            Debug.Log("invalid question.");
            TextUI.text = "invalid question.";
            return;
        }
        Debug.Log("Your Question:" + question + " " + "Your Answer: " + answer);

        TextUI.text = "Your Question:" + question + " " + "Your Answer: " + answer;

        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("Answers");

        Debug.Log("Running Transaction...");
        // Use a transaction to ensure that we do not encounter issues with
        // simultaneous updates that otherwise might create more than MaxScores top scores.
        reference.RunTransaction(AddQuestion)
          .ContinueWithOnMainThread(task =>
          {
              if (task.Exception != null)
              {
                  Debug.Log(task.Exception.ToString());
              }
              else if (task.IsCompleted)
              {
                  Debug.Log("Transaction complete.");
                  //content_1.SetActive.(false);
              }
          });
    }


    public void AuthenticateQuestion()
    {
        AddQuestion(questionInputField.text, MagicAnswer.AskAQuestion());
    }

    public void GoOfflineButton()
    {
        FirebaseDatabase.DefaultInstance.GoOffline();
        Debug.Log("You are Offline");
        TextUI.text = "You are Offline";
    }

    public void GoOnlineButton()
    {
        FirebaseDatabase.DefaultInstance.GoOnline();
        Debug.Log("You are online");
        TextUI.text = "You are Online";
    }

    IEnumerator QuestionCooldown()
    {
        yield return new WaitForSeconds(10f);
        canAsk = true;
    }
}