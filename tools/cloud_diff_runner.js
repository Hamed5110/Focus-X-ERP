(async function(){
try{
  var hdrs={};var fake={setRequestHeader:function(k,v){hdrs[k]=v;}};
  NETWORK.beforeSend(fake,{},null,null);
  var r=await fetch('/focusx/RD/RD/LoadRDDefinitionValue',{method:'POST',headers:{'Content-Type':'application/json; charset=UTF-8','X-Requested-With':'XMLHttpRequest','FOCUS':hdrs['FOCUS']},body:JSON.stringify({iReportId:70259}),credentials:'include'});
  var j=JSON.parse(await r.text());
  var d=j.data;
  var q=window.__diffQuery;
  d.TransactionSetData.sQuery=q;
  var r2=await fetch('/focusx/RD/RD/GetRDPreview',{method:'POST',headers:{'Content-Type':'application/json; charset=UTF-8','X-Requested-With':'XMLHttpRequest','FOCUS':hdrs['FOCUS']},body:JSON.stringify(d),credentials:'include'});
  var t=await r2.text();
  return JSON.stringify({status:r2.status,len:t.length,head:t.slice(0,600)});
}catch(e){return 'ERR: '+e.message;}
})()
