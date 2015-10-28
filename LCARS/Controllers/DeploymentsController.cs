﻿using System.Linq;
using System.Web.Mvc;
using LCARS.Domain;
using LCARS.Models;
using LCARS.ViewModels.Deployments;
using Environment = LCARS.ViewModels.Deployments.Environment;

namespace LCARS.Controllers
{
    public class DeploymentsController : Controller
    {
        private readonly ICommon _commonDomain;
        private readonly IDeployments _deploymentsDomain;
        private readonly Boards _thisBoard;

        public DeploymentsController(ICommon commonDomain, IDeployments deploymentsDomain)
        {
            _commonDomain = commonDomain;
            _deploymentsDomain = deploymentsDomain;
            _thisBoard = Boards.Deployments;
        }

        // GET: Deployments
        public ActionResult Index()
        {
            Boards randomBoard = _commonDomain.SelectBoard();

            if (_thisBoard != randomBoard)
            {
                return RedirectToAction("Index", randomBoard.GetDescription());
            }

            var deployments = _deploymentsDomain.Get().OrderBy(g => g.ProjectGroup).ThenBy(p => p.Project).ToList();

            var isRedAlertEnabled = _commonDomain.GetRedAlert(Server.MapPath(@"~/App_Data/RedAlert.xml")).IsEnabled;

            var projects =
                deployments.GroupBy(p => p.ProjectId)
                    .Select(
                        g =>
                            new Project
                            {
                                Id = g.First().ProjectId,
                                Name = g.First().Project.Replace("CasinoToolkit_", "")
                            })
                    .ToList();

            var environments =
                deployments.GroupBy(p => p.EnvironmentId)
                    .Select(
                        g =>
                            new Environment
                            {
                                Id = g.First().EnvironmentId,
                                Name = g.First().Environment
                            })
                    .ToList();

            environments =
                _deploymentsDomain.SetEnvironmentOrder(environments, Server.MapPath(@"~/App_Data/Deployments.xml"))
                    .ToList();

            var vm = new DeploymentStatus
            {
                Projects = projects,
                Environments = environments,
                Deployments = deployments,
                IsRedAlertEnabled = isRedAlertEnabled
            };

            return View(vm);
        }

        [HttpGet]
        public JsonResult GetStatus()
        {
            return Json(_deploymentsDomain.Get().ToList(), JsonRequestBehavior.AllowGet);
        }
    }
}