using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using PhotoToss.Core;
using Android.Support.V4.App;

namespace PhotoToss.AndroidApp
{
    public class SignInFragment : Android.Support.V4.App.Fragment
    {
        private EditText usernameField;
        private EditText passwordField;
        private EditText confirmPassword;
        private EditText emailField;
        private Button signInBtn;
        private Button createAccountBtn;
        private TextView prepSignIn;
        private TextView emailPrompt;
        private ProgressDialog progressDlg;


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.SignInLayout, container, false);

            progressDlg = new ProgressDialog(this.Activity);
            progressDlg.SetProgressStyle(ProgressDialogStyle.Spinner);

            view.FindViewById<TextView>(Resource.Id.textView1).SetTypeface(MainActivity.bodyFace, TypefaceStyle.Normal);
            usernameField = view.FindViewById<EditText>(Resource.Id.usernameField);
            usernameField.SetTypeface(MainActivity.bodyFace, TypefaceStyle.Normal);
            usernameField.AfterTextChanged += HandleTextValueChanged;

            passwordField = view.FindViewById<EditText>(Resource.Id.password);
            passwordField.SetTypeface(MainActivity.bodyFace, TypefaceStyle.Normal);
            passwordField.AfterTextChanged += HandleTextValueChanged;

            confirmPassword = view.FindViewById<EditText>(Resource.Id.password2);
            confirmPassword.SetTypeface(MainActivity.bodyFace, TypefaceStyle.Normal);
            confirmPassword.AfterTextChanged += HandleTextValueChanged;

            emailPrompt = view.FindViewById<TextView>(Resource.Id.emailPrompt);
            emailPrompt.SetTypeface(MainActivity.bodyFace, TypefaceStyle.Normal);

            emailField = view.FindViewById<EditText>(Resource.Id.emailAddrField);
            emailField.SetTypeface(MainActivity.bodyFace, TypefaceStyle.Normal);

            createAccountBtn = view.FindViewById<Button>(Resource.Id.createBtn);
            createAccountBtn.SetTypeface(MainActivity.bodyFace, TypefaceStyle.Normal);

            createAccountBtn.Click += (snder, e) =>
            {
                progressDlg.SetMessage("signing in...");
                progressDlg.Show();
                string userName = usernameField.Text.Trim();
                string password = passwordField.Text;
                signInBtn.Enabled = false;
                createAccountBtn.Enabled = false;


                // sign in
                PhotoTossRest.Instance.CreateAccount(userName, password, CreateAccountResultCallback);

            };

            prepSignIn = view.FindViewById<TextView>(Resource.Id.prepSignIn);
            prepSignIn.SetTypeface(MainActivity.bodyFace, TypefaceStyle.Normal);

            prepSignIn.Click += (object sender, EventArgs e) =>
            {
                PrepForSignIn();


            };

            signInBtn = view.FindViewById<Button>(Resource.Id.signInBtn);
            signInBtn.SetTypeface(MainActivity.bodyFace, TypefaceStyle.Normal);
            signInBtn.Visibility = ViewStates.Gone;

            signInBtn.Click += (object sender, EventArgs e) =>
            {
                progressDlg.SetMessage("signing in...");
                progressDlg.Show();
                string userName = usernameField.Text.Trim();
                string password = passwordField.Text;
                signInBtn.Enabled = false;
                createAccountBtn.Enabled = false;

                // sign in
                PhotoTossRest.Instance.Login(userName, password, SiginInResultCallback);
            };

            createAccountBtn.Enabled = false;
            signInBtn.Enabled = false;

            return view;

        }

        void HandleTextValueChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            string usernameText = usernameField.Text;
            string passwordText = passwordField.Text;
            string confirmText = confirmPassword.Text;
            string emailText = emailField.Text;

            if (String.IsNullOrEmpty(usernameText) || String.IsNullOrEmpty(passwordText) ||
                (usernameText.Length < 3) || (passwordText.Length < 3))
            {
                signInBtn.Enabled = false;
                createAccountBtn.Enabled = false;

            }
            else
            {
                signInBtn.Enabled = true;

                if (passwordText == confirmText)
                    createAccountBtn.Enabled = true;
                else
                    createAccountBtn.Enabled = false;
            }
        }

        private void SiginInResultCallback(User result)
        {

            if (result != null)
            {
                MainActivity.analytics.PostLogin();
                Activity.RunOnUiThread(() =>
                {
                    progressDlg.Hide();
                    ((FirstRunActivity)Activity).FinishSignin();
                });
            }
            else
            {
                MainActivity.analytics.PostSessionError("signinfailed");

                MainActivity.DisplayAlert(this.Activity, "Sign in Failed", "Unable to sign in.  Check username and password");
                Activity.RunOnUiThread(() =>
                {
                    progressDlg.Hide();
                    signInBtn.Enabled = true;
                    createAccountBtn.Enabled = true;
                    HandleTextValueChanged(null, null);
                });
            }

        }

        private void CreateAccountResultCallback(User result)
        {
            if (result != null)
            {
                MainActivity.analytics.PostRegisterUser();
                Activity.RunOnUiThread(() =>
                {
                    progressDlg.Hide();
                    string emailAddress = emailField.Text.Trim();

                    if (!String.IsNullOrEmpty(emailAddress))
                    {
                        PhotoTossRest.Instance.SetRecoveryEmail(emailAddress, (resultStr) =>
                        {
                            Activity.RunOnUiThread(() =>
                            {
                                ((FirstRunActivity)Activity).FinishCreateAccount();
                            });
                        });

                    }
                    else
                    {
                        ((FirstRunActivity)Activity).FinishCreateAccount();
                    }

                });
            }
            else
            {
                MainActivity.analytics.PostSessionError("registerfailed-");

                MainActivity.DisplayAlert(this.Activity, "Create Account Failed", "Unable to create account.  Check username");
                Activity.RunOnUiThread(() =>
                {
                    progressDlg.Hide();
                    signInBtn.Enabled = true;
                    createAccountBtn.Enabled = true;
                    HandleTextValueChanged(null, null);
                });
            }
        }

       

        void PrepForSignIn()
        {
            signInBtn.Visibility = ViewStates.Visible;
            confirmPassword.Visibility = ViewStates.Gone;
            emailPrompt.Visibility = ViewStates.Gone;
            emailField.Visibility = ViewStates.Gone;
            prepSignIn.Visibility = ViewStates.Gone;
            createAccountBtn.Visibility = ViewStates.Gone;
        }

       

        public override void OnStop()
        {
            progressDlg.Dismiss();
            base.OnStop();
        }

    }
}