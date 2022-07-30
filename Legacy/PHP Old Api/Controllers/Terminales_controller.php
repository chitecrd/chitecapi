<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use stdClass;
use Symfony\Component\HttpKernel\Exception\HttpException;
use DB;

class Terminales_controller extends Controller
{
    public function index(Request $request) 
    {
        $json = new stdClass(); 
        $es_request_valido = validar_request([/*'token',*/ 'DB'], $json, $request);
        if ($es_request_valido) 
        {
            $DB = ($request->input('DB') != NULL) ? $request->input('DB') : '';
            $existe_base_datos = DB::select('SELECT 1
             FROM INFORMATION_SCHEMA.SCHEMATA
             WHERE SCHEMA_NAME = ?
             LIMIT 1', [
                $DB
            ]);
            $existe_base_datos = (count($existe_base_datos) > 0);
            if ($existe_base_datos) 
            {
                //Validamos el token
                //if (validar_token($request->input('token'), $json, $DB)) 
                //{
                    $terminales = DB::select("SELECT * FROM {$DB}.terminal");
                    $json->data = $terminales;
                    $json->error = 0;
                    $json->error_type = 0;
                    $json->error_message = 0; 
                //}
            }   
            else 
            {
                $json->error = 1;
                $json->error_type = 400;
                $json->error_message = "La base de datos: \"{$DB}\" no existe";
            } 
        }

        if ($json->error_type != 400) 
        {
            return response()->json(
                $json,
                200
            );
        } else 
        {
            throw new HttpException(400, $json->error_message);
        }
    }
}