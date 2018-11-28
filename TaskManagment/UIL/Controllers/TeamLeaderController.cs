﻿using BLL;
using BOL;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace UIL.Controllers
{
    public class TeamLeaderController : ApiController
    {
        /// <summary>
        /// get the details and status of the current project that the teamLeader manage
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [Route("api/getProjectDeatails/{teamLeaderId}")]
        public HttpResponseMessage GetProjectDeatails(int teamLeaderId) => new HttpResponseMessage(HttpStatusCode.OK)
        {
          
            //curl -X GET -v http://localhost:59628/api/getProjectDeatails/5
            Content = new ObjectContent<List<Project>>(TeamLeaderLogic.GetProjectDeatails(teamLeaderId), new JsonMediaTypeFormatter())
        };
        /// <summary>
        /// get the workers in projects
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [Route("api/getWorkersDeatails/{teamLeaderId}")]
        public HttpResponseMessage GetWorkersDeatails(int teamLeaderId) => new HttpResponseMessage(HttpStatusCode.OK)
        {

            //curl -X GET -v http://localhost:59628/api/getWorkersDeatails/11
            Content = new ObjectContent<List<Worker>>(TeamLeaderLogic.GetWorkersDeatails(teamLeaderId), new JsonMediaTypeFormatter())
        };
        /// <summary>
        /// get the workers hours in projects
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [Route("api/getWorkersHours/{projectId}")]
        public HttpResponseMessage GetWorkersHours(int projectId) => new HttpResponseMessage(HttpStatusCode.OK)
        {

            //curl -X GET -v http://localhost:59628/api/getWorkersHours/11
            Content = new ObjectContent<List<Object>>(TeamLeaderLogic.getWorkersHours(projectId), new JsonMediaTypeFormatter())
        };


        /// <summary>
        /// get the worker hours for all projects
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [Route("api/getWorkerHours/{teamLeaderId}/{workerId}")]
        public HttpResponseMessage GetWorkerHours(int teamLeaderId, int workerId) => new HttpResponseMessage(HttpStatusCode.OK)
        {

            //curl -X GET -v http://localhost:59628/api/getWorkerHours/5/6
            Content = new ObjectContent<List<Object>>(TeamLeaderLogic.getWorkerHours(teamLeaderId,workerId), new JsonMediaTypeFormatter())
        };
        /// <summary>
        ///  get all the hours that used in current month
        /// </summary>

        [HttpGet]
        [Route("api/getHours/{projectId}")]
        public HttpResponseMessage GetHours(int projectId)
        //curl -X GET -v http://localhost:59628/api/GetHours/10/1
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ObjectContent<String>(TeamLeaderLogic.GetHours(projectId), new JsonMediaTypeFormatter())
            };
        }
        [HttpPut]
        [Route("api/updateWorkerHours")]
        public HttpResponseMessage UpdateWorkerHours([FromBody]JObject data)
        {
            int projectWorkerId=(int)data["projectWorkerId"];
            int numHours = (int)data["numHours"];

            //curl -v -X PUT -H "Content-type: application/json" -d "{\"projectWorkerId\":\"1\",\"numHours\":\"30\"}"  http://localhost:59628/api/updateWorkerHours
            return (TeamLeaderLogic.UpdateWorkerHours(projectWorkerId, numHours)) ?
           
                    new HttpResponseMessage(HttpStatusCode.OK) :
                    new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new ObjectContent<String>("Can not update in Data Base", new JsonMediaTypeFormatter())
                    };
          

           
        }
        /// <summary>
        /// get the remaining hours
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [Route("api/getRemainingHours/{projectId}/{jobId}")]
        public HttpResponseMessage GetRemainingHours(int projectId,int jobId) => new HttpResponseMessage(HttpStatusCode.OK)
        {

            //curl -X GET -v http://localhost:59628/api/getRemainingHours/
            Content = new ObjectContent<string>(TeamLeaderLogic.GetRemainingHours(projectId,jobId), new JsonMediaTypeFormatter())
        };

    }
}