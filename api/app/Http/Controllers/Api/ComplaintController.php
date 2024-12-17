<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Exceptions\Api\ForbiddenException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Complaint\ComplaintCreateRequest;
use App\Http\Requests\Api\Complaint\ComplaintUpdateRequest;
use App\Http\Resources\AlbumResource;
use App\Http\Resources\ComplaintResource;
use App\Models\Album;
use App\Models\Complaint;
use App\Models\Picture;
use Illuminate\Http\JsonResponse;
use Illuminate\Support\Facades\Auth;
use Illuminate\Http\Request;

class ComplaintController extends Controller
{
    public function index(Request $request): JsonResponse
    {
        $user = Auth::user();
        $status = $request->query('status'); // Получаем параметр status из запроса
        $sortBy = $request->query('sort', 'created');   // Сортировка по полю (по умолчанию дата)
        $orderBy = $request->has('reverse') ? 'desc' : 'asc'; // Направление сортировки
        $limit = intval($request->query('limit', 30)); // Лимит записей (по умолчанию 30)
        $limit_per_album = intval($request->query('limit_per_album', 10)); // Лимит записей жалоб внутри альбома(по умолчанию 10)

        // Проверка валидации сортировки
        $allowedSortFields = [
            'description' => 'description',
            'status' => 'status',
            'album' => 'id',
            'picture' => 'picture_id',
            'fromUser' => 'from_user_id',
            'type' => 'complaint_type_id',
            'created' => 'created_at',
            'updated' => 'updated_at',
        ];

        $keys = array_keys($allowedSortFields);
        if (!in_array($sortBy, $keys)) {
            throw new ApiException(
                'Sort must be of the following types: ' . join(', ', $keys),
                400
            );
        }

        // Запрос для альбомов
        $query = Album::query();

        if ($user->role->code !== 'admin') {
            // Пользователь видит только альбомы, на которые он пожаловался, исключая свои альбомы
            $query->whereHas('complaints', function ($q) use ($user) {
                $q->where('from_user_id', $user->id);
            });
        }
        // Фильтрация по статусу
        $query->whereHas('complaints', function ($q) use ($status) {
           if ($status === "null") {
               $q->whereNull('status'); // Не рассмотренные жалобы
           } else if($status == null) {
               return;
           }
           else {
               $q->where('status', $status); //Конкретный статус
           }
        });

        $query->with(['complaints' => function ($q) use (
            $limit,
            $user,
            $allowedSortFields,
            $sortBy,
            $orderBy,
            $request,
            $status,
            $limit_per_album,
        ) {

            if ($user->role->code !== 'admin') {
                $q->where('from_user_id', $user->id);
            }
            $q->with(['type', 'fromUser', 'picture'])
                ->orderBy($allowedSortFields[$sortBy], $orderBy)->limit($limit_per_album);

        }])->orderBy($allowedSortFields[$sortBy], $orderBy)
            ->withCount('complaints');

        // Пагинация
        $albumsPage = $query->paginate($limit);

        return response()->json([
            'page' => $albumsPage->currentPage(),
            'limit' => $albumsPage->perPage(),
            'total' => $albumsPage->total(),
            'albums' => AlbumResource::collection($albumsPage->items()),
        ]);
    }


    public function storeToPicture(ComplaintCreateRequest $request, Album $album, Picture $picture): JsonResponse
    {
        $user = Auth::user();
        $isAccessible = $album->usersViaAccess()->where('user_id', $user->id)->exists();
        if(!$isAccessible)
            throw new ForbiddenException();

        $isExisted = Complaint
            ::where('from_user_id', $user->id)
            ->where('picture_id', $picture->id)
            ->exists();
        if ($isExisted)
            throw new ApiException('You are already complain to this picture', 409);

        Complaint::create([
            'picture_id'        => $picture->id,
            'album_id'          => $album->id,
            'description'       => $request->input('description'),
            'complaint_type_id' => $request->input('typeId'),
            'from_user_id'      => $user->id,
            'about_user_id'     => $album->user_id,
        ]);
        return response()->json(null, 204);
    }

    public function storeToAlbum(ComplaintCreateRequest $request, Album $album): JsonResponse
    {
        $user = Auth::user();
        $isAccessible = $album->usersViaAccess()->where('user_id', $user->id)->exists();
        if(!$isAccessible)
            throw new ForbiddenException();

        $isExisted = Complaint
            ::where('from_user_id', $user->id)
            ->where('album_id', $album->id)
            ->exists();
        if($isExisted)
            throw new ApiException('You are already complain to this album', 409);

        Complaint::create([
            'album_id'          => $album->id,
            'description'       => $request->input('description'),
            'complaint_type_id' => $request->input('typeId'),
            'from_user_id'      => $user->id,
            'about_user_id'     => $album->user_id,
        ]);
        return response()->json(null, 204);
    }

    public function updateBatch(Complaint $complaint, ComplaintUpdateRequest $request): JsonResponse
    {
        $complaints = Complaint::with(['type', 'aboutUser', 'fromUser', 'picture', 'album'])
            ->where('about_user_id', $complaint->about_user_id)
            ->where('album_id'     , $complaint->album_id)
            ->orWhere('picture_id' , $complaint->picture_id)
            ->get();
        foreach ($complaints as $complaint) {
            $complaint->status = $request->input('status');
            $complaint->save();
        }
        return response()->json(['complaints' => ComplaintResource::collection($complaints)]);
    }

    public function destroy(Complaint $complaint): JsonResponse
    {
        $user = Auth::user();
        if ($complaint->from_user_id !== $user->id)
            throw new ForbiddenException();

        $complaint->delete();
        return response()->json(null, 204);
    }
}
