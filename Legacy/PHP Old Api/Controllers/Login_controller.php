<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use stdClass;
use Symfony\Component\HttpKernel\Exception\HttpException;
use Hash;
use DB;
use App\Tokens;

class Login_controller extends Controller
{
    public function login(Request $request) 
    {
        $request = json_decode(file_get_contents('php://input'), true);
        $json = new stdClass(); 
        if ($request != NULL) 
        {
            $es_request_valido = validar_request_json(['usuario', 'password', 'DB'], $json, $request);
            if ($es_request_valido) 
            {
                $DB = $request['DB'];
                $existe_base_datos = DB::select('SELECT 1
                 FROM INFORMATION_SCHEMA.SCHEMATA
                 WHERE SCHEMA_NAME = ?
                 LIMIT 1', [
                    $DB
                ]);
                $existe_base_datos = (count($existe_base_datos) > 0);
                if ($existe_base_datos) 
                {
                    $usuario = $request['usuario'];
                    $password = $request['password'];
        
                    $usuario = DB::select("SELECT *
                    FROM {$DB}.usuarios 
                    WHERE usuario = ?
                    LIMIT 1", [
                        $usuario
                    ]);
                    
                    //Autenticamos el usuario
                   // if (count($usuario) > 0 && base64_encode('chitec_2020_' .$password) == $usuario[0]->password)
                   if (count($usuario) > 0 && $password == $usuario[0]->password) 
                    {
                        $es_cuenta_activa = DB::select("SELECT 1 FROM {$DB}.usuarios 
                        WHERE id = ? AND estado = 1 LIMIT 1", [
                            $usuario[0]->id
                        ]);
                        $es_cuenta_activa = (count($es_cuenta_activa) > 0);
                        if ($es_cuenta_activa) 
                        {
                            $json = $usuario[0];
                            //$json->password = $json->password;
                            //Generamos el token
                            $token = $this->get_token($DB);
        
                            //Guardamos el token
                            $es_token_guardado = $this->guardar_token($token, $json->id, $DB);
                            if ($es_token_guardado) 
                            {
                                $json->token = $token;
                                $json->error = 0;
                                $json->error_type = 0;
                                $json->error_message = '';
                            } 
                            else 
                            {
                                $json = new stdClass();
                                $json->error = 1;
                                $json->error_type = 400;
                                $json->error_message = 'Ups, algo ha salido mal. Por favor, intenta de nuevo más tarde';
                            }
                        }
                        else 
                        {
                            $json->error = 1;
                            $json->error_type = 2;
                            $json->error_message = 'Su cuenta está inactiva';
                        }
                    }
                    else 
                    {
                        $json->error = 1;
                        $json->error_type = 1;
                        $json->error_message = 'Verifique el usuario o clave';
                    }
                }
                else 
                {
                    $json->error = 1;
                    $json->error_type = 400;
                    $json->error_message = "La base de datos: \"{$DB}\" no existe";
                } 
            }
        }
        else 
        {
            $json->error = 1;
            $json->error_type = 400;
            $json->error_message = 'Faltan los parámetros';
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

    public function login_web(Request $request) 
    {
        $request = json_decode(file_get_contents('php://input'), true);
        $json = new stdClass(); 
        if ($request != NULL) 
        {
            $es_request_valido = validar_request_json(['email', 'password'], $json, $request);
            if ($es_request_valido) 
            {
                $usuario = DB::select("SELECT u.*, e.nombre as nombre_empresa 
                 FROM usuarios u
                 inner join empresas e
                  on (u.id_empresa = e.id)
                 WHERE u.usuario = ? AND u.password = ?
                 LIMIT 1", [
                    $request['email'],
                    $request['password']
                ]);
                //Autenticamos
                if (count($usuario) > 0) 
                {
                    $usuario = $usuario[0];
                    if ($usuario->estado == 1) 
                    {
                        $json->usuario = $usuario;
                        $json->error = 0;
                        $json->error_type = 0;
                        $json->error_message = '';
                    }
                    else 
                    {
                        $json->error = 1;
                        $json->error_type = 2;
                        $json->error_message = 'El usuario no está activo. Comuniquese con la administración';
                    }
                }
                else 
                {
                    $json->error = 1;
                    $json->error_type = 1;
                    $json->error_message = 'Verifique el usuario o clave..';
                }
            }
        }
        else 
        {
            $json->error = 1;
            $json->error_type = 400;
            $json->error_message = 'Faltan los parámetros (email, password)';
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

    //Metodos privados
    private function get_token($DB = '') 
    {
        $token = '';
        while (true) 
        {
            $token =  bin2hex(openssl_random_pseudo_bytes(64));
            //Validamos si existe el token en la base de datos (si existe, generamos otra token).
            $existe_token = Tokens::validar_token($token, $DB);
            if (!$existe_token) 
            {
                break;
            }
        }
        return $token;
    }

    private function guardar_token($token, $id_usuario, $DB = '') 
    {
        $fecha_hoy = date('Y-m-d H:i:s');
        $nombre_tabla = ($DB != '') ? "{$DB}.tokens" : "tokens";
        $es_guardado = DB::insert("INSERT INTO {$nombre_tabla} (uuid, token, user_id, state, created_at, updated_at)
         VALUES (?,?,?,1,?,?)", [
            uniqid(),
            $token,
            $id_usuario,
            $fecha_hoy,
            $fecha_hoy
        ]);
        return $es_guardado;
    }
}
