using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.Accounts;
using Amazon.AspNetCore.Identity.Cognito;

namespace WebAdvert.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;

        public AccountsController(SignInManager<CognitoUser> signInManager, UserManager<CognitoUser> userManager, CognitoUserPool pool)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _pool = pool;
        }



        // GET: AccountsController
        public async Task<ActionResult> SignUp()
        {
            var model = new SignUpModel();
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> SignUp(SignUpModel signUpModel)
        {
            if (ModelState.IsValid)
            {
                var user = _pool.GetUser(signUpModel.Email);

                if (user.Status != null)
                {
                    ModelState.AddModelError("User Exists", "User with Email already exists");
                    return View(signUpModel);
                }

                user.Attributes.Add(CognitoAttribute.Name.ToString(), signUpModel.Email);
                var createUser = await _userManager.CreateAsync(user, signUpModel.Password).ConfigureAwait(false);
                if (createUser.Succeeded)
                {
                    return RedirectToAction("Confirm");
                }
            }
            return View(signUpModel);
        }

        public async Task<ActionResult> Confirm()
        {
            var model = new ConfirmModel();
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Confirm(ConfirmModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
                if (user == null)
                {
                    ModelState.AddModelError("NotFound", "A user with email not found");
                    return View(model);
                }
                var result = await ((CognitoUserManager<CognitoUser>)_userManager).ConfirmSignUpAsync(user, model.Code, true);
                //var result = await _userManager.ConfirmEmailAsync(user, model.Code).ConfigureAwait(false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }
                    return View(model);
                }
            }
            return View(model);
        }


        public async Task<ActionResult> Login()
        {
            return View(new LoginModel());
        }

        [HttpPost]
        [ActionName("Login")]
        public async Task<ActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.Remember, false).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("Login Error", "Email or Password not match");
                }
            }
            return View(model);
        }

    }
}
