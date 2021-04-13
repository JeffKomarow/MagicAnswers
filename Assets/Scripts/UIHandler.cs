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
// How to properly end the animation so it isn's looping, but returns to the start once the start is called.
// Play sound when transaction is complete
// Close and open the right panels.


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

    public TMP_Text textUI;

    public TMP_InputField questionInputField;

    public GameObject content_0_Ask;
    public GameObject content_1_Answer;
    public GameObject content_2_Leaderboard;
    public GameObject content_3_IntroText;

    public TMP_Text questionUI;
    public TMP_Text answerUI;

    public Transform leaderBoardArea;
    public GameObject rowPreFab;

    public GameObject magicalSFX;


    // At start, check for the required dependencies to use Firebase, and if not, add them if possible.
    protected virtual void Start()
    {
        canAsk = true;
        questions.Clear();
        questions.Add("Firebase Top " + _maxAnswers.ToString() + " Scores");

        content_0_Ask.SetActive(false);

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
        textUI.text = "The underworld is ready for your question " + _user + ".";
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
            textUI.text = "invalid question.";
            return;
        }
        Debug.Log("Your Question:" + question + " " + "Your Answer: " + answer);
        content_1_Answer.SetActive(true);
        content_0_Ask.SetActive(false);
        content_3_IntroText.SetActive(false);
        Instantiate(magicalSFX);
        questionUI.text = question;
        answerUI.text = answer;



        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("Answers");

        Debug.Log("Running Transaction...");

        // Use a transaction to ensure that we do not encounter issues with simultaneous updates.
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

    IEnumerator QuestionCooldown()
    {
        yield return new WaitForSeconds(60f);
        canAsk = true;
    }
}