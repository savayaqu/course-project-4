<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\Album;
use App\Models\Picture;
use Carbon\Carbon;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Storage;

class PictureController extends Controller
{
    public function create(Request $request, $album_id)
    {
        $user = Auth::user();
        $files = $request->file('images');
        $album = Album::where('id', $album_id)->where('user_id', $user->id)->first();

        $path = $user->login.'/albums/'.$album->id.'/pictures/';
        $responses = [];
        foreach ($files as $file) {
            $filename = $file->getClientOriginalName();
            $imageHash = sha1_file($file->getRealPath());

            $image = Picture::where('album_id', $album_id)->where('hash', $imageHash)->first();
            if ($image) {
                $responses['errored'][] = [
                    'name' => $filename,
                    'message' => 'already exist in this album'
                ];
                continue;
            }
            $file->storeAs($path, $filename);
            $sizes = getimagesize(Storage::path($path.$filename));

            $imageDB = Picture::create([
                'name' => $filename,
                'path' => $path.$filename,
                'hash' => $imageHash,
                'preview',
                'date' => Carbon::createFromTimestamp(Storage::lastModified($path.$filename)),
                'size' => Storage::size($path.$filename),
                'width' => $sizes[0],
                'height' => $sizes[1],
                'user_id' => $user->id,
                'album_id' => $album_id,
            ]);
            $responses['successful'][] = $imageDB;
        }
        return response($responses);
    }



}
